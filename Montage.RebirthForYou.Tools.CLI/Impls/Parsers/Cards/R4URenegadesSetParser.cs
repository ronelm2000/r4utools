using AngleSharp.Html.Dom;
using AngleSharp.Css;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using System.Linq;
using Lamar;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class R4URenegadesSetParser : ICardSetParser
    {
        private ILogger Log = Serilog.Log.ForContext<InternalSetParser>();

        private Regex serialRarityJPNameMatcher = new Regex(@"([^ ]+) ([A-Za-z0-9]+) (.*)(?:(?: ?)<strong>)(.+)(?:<\/strong>)(?:<br>|$)");
        private Regex costSeriesTraitMatcher = new Regex(@"(?:Cost )([0-9]+)(?: \/ )(.+)(?: \/ )(.+)");
        private Regex seriesRebirthMatcher = new Regex(@"(.+) \/ (.+) Rebirth");
        private Regex rubyMatcher = new Regex(@"(<rt>)([^>]+)(<\/rt>)|(<ruby>)|(<\/ruby>)");
        private Regex releaseIDMatcher = new Regex(@"(([A-Za-z0-9]+)(\/)([^-]+))-");
        private Regex overflowEffectTextMatcher = new Regex(@"^((\()|i\.|ii\.|iii\.|iv\.)");

//        private Func<Task<Dictionary<string, R4UReleaseSet>>> _sets;

        /*
        public R4URenegadesSetParser()
        {
            _sets = async () => new Dictionary<string, R4UReleaseSet>();
        }

        public R4URenegadesSetParser (IContainer ioc)
        {
            _sets = async () =>
            {
                using (var db = ioc.GetInstance<CardDatabaseContext>())
                {
                    return await db.R4UReleaseSets.ToDictionaryAsync(s => s.ReleaseCode);
                }
            };
        }
        
        */

        public bool IsCompatible(IParseInfo parseInfo)
        {
            return parseInfo.URI.StartsWith("https://rebirthforyourenegades.wordpress.com/");
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            Log.Information("Starting...");
            var document = await new Uri(urlOrLocalFile).DownloadHTML(("Referer", "rebirthforyourenegades.wordpress.com")).WithRetries(10);
            var postContent = document.QuerySelector(".post-content");
            var setMap = new Dictionary<string, R4UReleaseSet>();
            var allDivs = postContent.QuerySelectorAll(".wp-block-image").AsEnumerable();
            allDivs = allDivs.Concat(postContent.QuerySelectorAll(".wp-block-jetpack-slideshow"));
            foreach (var figure in allDivs)
            {
                Log.Information("Starting...");
                var paragraph = figure.NextElementSibling as IHtmlParagraphElement;
                if (paragraph == null)
                    throw new NotImplementedException("There should have been a <p> tag after the <figure> tag, but instead found nothing.");
                foreach (var card in CreateBaseCards(paragraph, setMap))
                    yield return card;
            }
            //throw new NotImplementedException();
        }

        private IEnumerable<R4UCard> CreateBaseCards(IHtmlParagraphElement paragraph, Dictionary<string, R4UReleaseSet> setMap)
        {
            List<R4UCard> cards = new List<R4UCard>();
            cards.Add(new R4UCard());

            var card = cards.First();
            var content = paragraph.InnerHtml;
            var text = paragraph.GetInnerText();
            var cursor = text.AsSpanCursor();
            // var space = " ";

            // Format: <Serial> <Rarity> <JP Name with Spaces> <strong>English Name with Spaces</strong><br>
            if (TryGetExceptionalSerialRarityName(cursor.CurrentLine.ToString(), out var exceptionalResults))
            {
                cards = exceptionalResults.Select(res =>
                {
                    var card = new R4UCard();
                    card.Serial = res.Serial;
                    card.Rarity = res.Rarity;
                    card.Name = res.Name;
                    return card;
                }).ToList();
                card = cards.First();
            }
            else if (serialRarityJPNameMatcher.IsMatch(content))
            {
                var firstLineMatch = serialRarityJPNameMatcher.Match(content);
                card.Serial = firstLineMatch.Groups[1].Value.Trim();
                card.Rarity = firstLineMatch.Groups[2].Value.Trim();
                card.Name = new MultiLanguageString();
                card.Name.JP = rubyMatcher.Replace(firstLineMatch.Groups[3].Value, "").Trim(); // TODO: Resolve <ruby>永<rt>えい</rt>遠<rt>えん</rt></ruby>の<ruby>巫<rt>み</rt>女<rt>こ</rt></ruby> <ruby>霊<rt>れい</rt>夢<rt>む</rt></ruby>
                card.Name.EN = firstLineMatch.Groups[4].Value.Trim();
            }
            else
                throw new NotImplementedException($"The serial/rarity/JP Name line cannot be parsed. Here's the offending line: {cursor.CurrentLine.ToString()}");

            var releaseID = releaseIDMatcher.Match(card.Serial).Groups[1].Value;
            card.Set = setMap.GetValueOrDefault(releaseID, null) ?? CreateTemporarySet(releaseID);

            // Format: Cost <Cost> / <Series Name> / <Traits>
            cursor.Next();
            var secondLine = cursor.CurrentLine.ToString();
            if (costSeriesTraitMatcher.IsMatch(secondLine))
            {
                card.Type = CardType.Character;
                var secondLineMatch = costSeriesTraitMatcher.Match(cursor.CurrentLine.ToString());
                card.Cost = int.Parse(secondLineMatch.Groups[1].Value);
                card.Traits = secondLineMatch.Groups[3].Value //
                    .Split(" – ") //
                    .Select(str => str.Trim()) //
                    .Select(t => new MultiLanguageString() { EN = t }) //
                    .ToList();

                cursor.Next();
                card.ATK = cursor.CurrentLine
                    .Slice("ATK ".Length)
                    .AsParsed<int>(int.TryParse);

                cursor.Next();
                string defLine = cursor.CurrentLine.ToString();
                card.DEF = cursor.CurrentLine
                    .Slice("DEF ".Length)
                    .AsParsed<int>(int.TryParse);

                Regex flavorTextMatcher = new Regex(@"" + defLine + @"<br><em>(.+)</em><br>");
                if (flavorTextMatcher.IsMatch(content))
                {
                    cursor.Next();
                    card.Flavor = new MultiLanguageString();
                    card.Flavor.EN = cursor.CurrentLine.ToString(); // flavorTextMatcher.Match(content).Groups[1].Value;
                }
            }
            else if (seriesRebirthMatcher.IsMatch(secondLine))
            {
                card.Type = CardType.Rebirth;
                var rebirthLine = secondLine;

                Regex flavorTextMatcher = new Regex(@"" + rebirthLine + @"<br><em>(.+)</em><br>");
                if (flavorTextMatcher.IsMatch(content))
                {
                    cursor.Next();
                    card.Flavor = new MultiLanguageString();
                    card.Flavor.EN = cursor.CurrentLine.ToString(); // flavorTextMatcher.Match(content).Groups[1].Value;
                }
            }
            else
            {
                card.Type = CardType.Partner;
                card.Color = CardColor.Red;
            }

            if (card.Color != CardColor.Red)
            {
                List<MultiLanguageString> effects = new List<MultiLanguageString>();
                while (cursor.Next())
                {
                    Log.Information("Adding Effect: {eff}", cursor.CurrentLine.ToString());
                    effects.Add(new MultiLanguageString() { EN = cursor.CurrentLine.ToString() });
                }
                effects = Compress(effects);
                card.Effect = effects.ToArray();
                card.Color = CardUtils.InferFromEffect(card.Effect);
            }

            yield return card;
            foreach (var dupCard in cards.Skip(1))
            {
                var detailedDupCard = card.Clone();
                detailedDupCard.Serial = dupCard.Serial;
                detailedDupCard.Rarity = dupCard.Rarity;
                detailedDupCard.Name = dupCard.Name;
                yield return detailedDupCard;
            }
        }

        private bool TryGetExceptionalSerialRarityName(string line, out (string Serial, string Rarity, MultiLanguageString Name)[] exceptionalResult)
        {
            exceptionalResult = line switch
            {
                "IMC/001B-023 喜多見 柚 Yuzu Kitami"
                    => new[] { (Serial: "IMC/001B-023", Rarity: "C", Name: new MultiLanguageString { EN = "Yuzu Kitami", JP = "喜多見 柚" }) },
                "HP/001B-029 にんまりきーつね フブキ Fox’s Satisfied Grin, Fubuki" 
                    => new[] { (Serial: "HP/001B-029", Rarity: "R", Name: new MultiLanguageString { EN = "Fox’s Satisfied Grin, Fubuki", JP = "にんまりきーつね フブキ" }) },
                "HP/001B-059 R 僕でいいじゃん おかゆ Aren’t you fine with me, Okayu" 
                    => new[] { (Serial: "HP/001B-059", Rarity: "R", Name: new MultiLanguageString { EN = "Aren’t you fine with me, Okayu", JP = "僕でいいじゃん おかゆ" }) },
                "HP/001B-060 てぇてぇ おかゆ Wholesome, Okayu"
                    => new[] { (Serial: "HP/001B-060", Rarity: "C", Name: new MultiLanguageString { EN = "Wholesome, Okayu", JP = "てぇてぇ おかゆ" }) },
                "HP-001B/107 ReR ハイテンションサマー Full Throttle Summer"                               
                    => new[] { (Serial: "HP/001B-107", Rarity: "ReR", Name: new MultiLanguageString { EN = "Full Throttle Summer", JP = "ハイテンションサマー" }) },
                "HP/001B-091 R 勝てるかな？ わため Can I Win? Watame"
                    => new[]
                    {
                        (Serial: "HP/001B-091a", Rarity: "R", Name: new MultiLanguageString { EN = "勝てるかな？ わため", JP = "Can I Win? Watame" }),
                        (Serial: "HP/001B-091b", Rarity: "R", Name: new MultiLanguageString { EN = "勝てるかな？ わため", JP = "Can I Win? Watame" }),
                        (Serial: "HP/001B-091c", Rarity: "R", Name: new MultiLanguageString { EN = "勝てるかな？ わため", JP = "Can I Win? Watame" })
                    },
                _ => null
            };  
            return exceptionalResult != null;
        }

        private List<MultiLanguageString> Compress(List<MultiLanguageString> effects)
        {
            var overflowEffects = effects   .Select((mls, i) => (Effect: mls, Index: i)) //
                                            .Where((pair) => overflowEffectTextMatcher.IsMatch(pair.Effect.EN))
                                            .ToDictionary((pair) => pair.Index, (pair) => pair.Effect);
            List<MultiLanguageString> newEffects = effects.Select((mls, i) => (Effect: mls, Index: i)) //
                                                            .Where((pair) => !overflowEffectTextMatcher.IsMatch(pair.Effect.EN))
                                                            .Select((pair) => GroupConcat(pair, overflowEffects))
                                                            .ToList(); //.ToArray();
            return newEffects;

            MultiLanguageString GroupConcat((MultiLanguageString Effect, int Index) pair, Dictionary<int, MultiLanguageString> overflowEffects)
            {
                if (overflowEffects.Count < 1) return pair.Effect;
                MultiLanguageString newEffect = pair.Effect.Clone();
                for (int i = pair.Index; i < overflowEffects.Keys.Max(); i++)
                {
                    if (overflowEffects.ContainsKey(i + 1))
                        newEffect.EN += "\n" + overflowEffects[i + 1].EN;
                    else
                        break;
                }
                return newEffect;
            }
        }

        private R4UReleaseSet CreateTemporarySet(string releaseCode)
        {
            return new R4UReleaseSet()
            {
                ReleaseCode = releaseCode
            };
        }

    }
}
