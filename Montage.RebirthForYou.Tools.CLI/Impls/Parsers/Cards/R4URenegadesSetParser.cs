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
        private readonly ILogger Log = Serilog.Log.ForContext<R4URenegadesSetParser>();

        private readonly Regex serialRarityJPNameMatcher = new(@"([^ ]+) ([A-Za-z0-9]+) (.*)(?:(?: ?)<strong>)(.+)(?:<br><\/strong>|<\/strong><br>|<\/strong>$)");
        private readonly Regex serialTrialJPNameMatcher = new(@"((?:\w)+\/(?:\d)+T-(?:\d)+) (.*)(?:(?: ?)<strong>)(.+)(?:<br><\/strong>|<\/strong><br>)");
        private readonly Regex costSeriesTraitMatcher = new(@"(?:Cost )([0-9]+)(?: \/ )(.+)(?: \/ )(.+)");
        private readonly Regex seriesRebirthMatcher = new(@"(.+) \/ (.+) Rebirth");
        private readonly Regex rubyMatcher = new(@"(<rt>)([^>]+)(<\/rt>)|(<ruby>)|(<\/ruby>)");
        private readonly Regex releaseIDMatcher = new(@"(([A-Za-z0-9]+)(\/)([^-]+))-");
        private readonly Regex overflowEffectTextMatcher = new(@"^(?=(\()|i\.|ii\.|iii\.|iv\.|(\d+) or more:).+");

        public bool IsCompatible(IParseInfo parseInfo)
        {
            return parseInfo.URI.StartsWith("https://rebirthforyourenegades.wordpress.com/");
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            Log.Information("Starting...");
            var uri = new Uri(urlOrLocalFile);
            Log.Debug("URI: {url}", uri);
            var document = await uri.DownloadHTML(("Referer", "rebirthforyourenegades.wordpress.com")).WithRetries(10);
            IAsyncEnumerable<R4UCard> result;
            if (document.QuerySelector(".page-title") != null)
                result = ParseTagPage(document);
            else
                result = ParseSetListPage(document);
            await foreach (var card in result)
                yield return card;
        }

        private IAsyncEnumerable<R4UCard> ParseSetListPage(IDocument document)
        {
            Log.Information("Parsing as Set List page...");
            var postContent = document.QuerySelector(".post-content");
            var setMap = new Dictionary<string, R4UReleaseSet>();
            var allDivs = postContent.QuerySelectorAll(".wp-block-image").AsEnumerable();
            allDivs = allDivs.Concat(postContent.QuerySelectorAll(".wp-block-jetpack-slideshow"));
            return allDivs.ToAsyncEnumerable().SelectMany(f => CreateBaseCards(f, setMap).ToAsyncEnumerable());
        }

        private IAsyncEnumerable<R4UCard> ParseTagPage(IDocument document)
        {
            Log.Information("Parsing as Tag page...");
            var setMap = new Dictionary<string, R4UReleaseSet>();
            return document.QuerySelectorAll(".tag-cotd").AsEnumerable()
                .Select(a => a.QuerySelector<IHtmlAnchorElement>(".post-title > a").Href)
                .ToAsyncEnumerable()
                .SelectAwait(async a => await new Uri(a).DownloadHTML(("Referer", "rebirthforyourenegades.wordpress.com")).WithRetries(10))
                .SelectMany(d => d.QuerySelectorAll(".wp-block-image").Concat(d.QuerySelectorAll(".wp-block-jetpack-slideshow")).ToAsyncEnumerable())
                .SelectMany(f => CreateBaseCards(f, setMap).ToAsyncEnumerable())
                ;
        }

        private IEnumerable<R4UCard> CreateBaseCards(IElement figureOrImage, Dictionary<string, R4UReleaseSet> setMap)
        {
            if (figureOrImage.NextElementSibling is not IHtmlParagraphElement paragraph)
                throw new NotImplementedException("There should have been a <p> tag after the <figure> tag, but instead found nothing.");
            return CreateBaseCards(paragraph, setMap);
        }

        private IEnumerable<R4UCard> CreateBaseCards(IHtmlParagraphElement paragraph, Dictionary<string, R4UReleaseSet> setMap)
        {
            List<R4UCard> cards = new()
            {
                new R4UCard()
            };

            var card = cards.First();
            var content = paragraph.InnerHtml;
            var text = paragraph.GetInnerText();
            var cursor = text.AsSpanCursor();

            Log.Information("Parsing Content: {div} [...]", content.Take(30).Aggregate("", (s, c) => s + c));

            // var space = " ";

            // Format: <Serial> <Rarity> <JP Name with Spaces> <strong>English Name with Spaces</strong><br>
            if (TryGetExceptionalSerialRarityName(cursor.CurrentLine.ToString(), out var exceptionalResults))
            {
                cards = exceptionalResults.Select(res =>
                {
                    var card = new R4UCard
                    {
                        Serial = res.Serial,
                        Rarity = res.Rarity,
                        Name = res.Name
                    };
                    return card;
                }).ToList();
                if (cards.Count < 1) yield break;
                card = cards.First();
            }
            else if (serialRarityJPNameMatcher.IsMatch(content))
            {
                var firstLineMatch = serialRarityJPNameMatcher.Match(content);
                card.Serial = firstLineMatch.Groups[1].Value.Trim();
                card.Rarity = firstLineMatch.Groups[2].Value.Trim();
                card.Name = new MultiLanguageString
                {
                    JP = rubyMatcher.Replace(firstLineMatch.Groups[3].Value, "").Trim(), // TODO: Resolve <ruby>永<rt>えい</rt>遠<rt>えん</rt></ruby>の<ruby>巫<rt>み</rt>女<rt>こ</rt></ruby> <ruby>霊<rt>れい</rt>夢<rt>む</rt></ruby>
                    EN = firstLineMatch.Groups[4].Value.Trim()
                };
            }
            else if (serialTrialJPNameMatcher.IsMatch(content))
            {
                var firstLineMatch = serialTrialJPNameMatcher.Match(content);
                card.Serial = firstLineMatch.Groups[1].Value.Trim();
                card.Rarity = "TD";
                card.Name = new MultiLanguageString
                {
                    JP = rubyMatcher.Replace(firstLineMatch.Groups[2].Value, "").Trim(), // TODO: Resolve <ruby>永<rt>えい</rt>遠<rt>えん</rt></ruby>の<ruby>巫<rt>み</rt>女<rt>こ</rt></ruby> <ruby>霊<rt>れい</rt>夢<rt>む</rt></ruby>
                    EN = firstLineMatch.Groups[3].Value.Trim()
                };
            }
            else
                throw new NotImplementedException($"The serial/rarity/JP Name line cannot be parsed. Here's the offending line: {cursor.CurrentLine.ToString()}");

            var releaseID = releaseIDMatcher.Match(card.Serial).Groups[1].Value;
            card.Set = setMap.GetValueOrDefault(releaseID, null) ?? CreateTemporarySet(releaseID);

            // Format: Cost <Cost> / <Series Name> / <Traits>
            cursor.Next();
            var secondLine = cursor.CurrentLine.ToString();
            if (TryGetExceptionalCostTraitLine(secondLine, out var errataResult)) {
                card.Type = errataResult?.Type;
                card.Cost = errataResult?.Cost;
                card.Traits = errataResult?.Traits.Select(te => new MultiLanguageString { EN = te }).ToList();
            }
            else if (costSeriesTraitMatcher.IsMatch(secondLine))
            {
                card.Type = CardType.Character;
                var secondLineMatch = costSeriesTraitMatcher.Match(cursor.CurrentLine.ToString());
                card.Cost = int.Parse(secondLineMatch.Groups[1].Value);
                card.Traits = secondLineMatch.Groups[3].Value //
                    .Split(" – ") //
                    .Select(str => str.Trim()) //
                    .Where(str => str != "(Traitless)") //
                    .Select(t => new MultiLanguageString() { EN = t }) //
                    .ToList();
            }
            else if (seriesRebirthMatcher.IsMatch(secondLine))
            {
                card.Type = CardType.Rebirth;
                var rebirthLine = secondLine;

                Regex flavorTextMatcher = new(@"" + rebirthLine + @"<br><em>(.+)</em><br>");
                if (flavorTextMatcher.IsMatch(content))
                {
                    cursor.Next();
                    card.Flavor = new MultiLanguageString
                    {
                        EN = cursor.CurrentLine.ToString() // flavorTextMatcher.Match(content).Groups[1].Value;
                    };
                }
            }
            else
            {
                card.Type = CardType.Partner;
                card.Color = CardColor.Red;
            }

            if (card.Type == CardType.Character)
            { 
                cursor.Next();
                card.ATK = cursor.CurrentLine["ATK ".Length..]
                    .AsParsed<int>(int.TryParse);

                cursor.Next();
                string defLine = cursor.CurrentLine.ToString();
                card.DEF = cursor.CurrentLine["DEF ".Length..]
                    .AsParsed<int>(int.TryParse);

                Regex flavorTextMatcher = new(@"" + defLine + @"<br><em>(.+)</em><br>");
                if (flavorTextMatcher.IsMatch(content))
                {
                    cursor.Next();
                    card.Flavor = new MultiLanguageString
                    {
                        EN = cursor.CurrentLine.ToString() // flavorTextMatcher.Match(content).Groups[1].Value;
                    };
                }
            }

            if (card.Color != CardColor.Red)
            {
                List<MultiLanguageString> effects = new();
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

        private bool TryGetExceptionalCostTraitLine(string line, out (int? Cost, CardType Type, string[] Traits)? errata)
        {
            errata = line switch
            {
                "Cost 2/ Rebirth / Rebirth – Go Go Stew’s!" => (
                    Cost: 2,
                    Type: CardType.Character,
                    Traits: new[] { "Rebirth", "Go Go Stew's!" }
                    ),
                _ => null
            };
            return errata != null;
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
                "GP/002B-061 RR大盛り一丁！ 花音 A Large Serving! Kanon"
                    => new[] { (Serial: "GP/002B-061", Rarity: "RR", Name: new MultiLanguageString { EN = "A Large Serving! Kanon", JP = "大盛り一丁！ 花音" }) },
                "DJ/001T-004 TD D4 FES.を目指して！ 真秀 Aiming for the D4 FES.! Maho"
                    => new[] { (Serial: "DJ/001T-004", Rarity: "TD", Name: new MultiLanguageString { EN = "Aiming for the D4 FES.! Maho", JP = "D4 FES.を目指して！ 真秀" }) },
                "DJ/002T-P08 TD三宅葵依 Aoi Miyake"
                    => new[] { (Serial: "DJ/002T-P08", Rarity: "TD", Name: new MultiLanguageString { EN = "Aoi Miyake", JP = "三宅葵依" }) },
                "DJ/001B-089 楽しいことが大好き みいこ Loving Fun Things, Miiko"
                    => new[] { (Serial: "DJ/001B-089", Rarity: "C", Name: new MultiLanguageString { EN = "Loving Fun Things, Miiko", JP = "楽しいことが大好き みいこ" }) },
                "DJ/001B-092 Pop◎コーヒーカップ Pop Coffee Mug"
                    => new[] { (Serial: "DJ/001B-092", Rarity: "ReR", Name: new MultiLanguageString { EN = "Pop Coffee Mug", JP = "Pop◎コーヒーカップ" }) },
                "SSSS/001T-083 C 幽愁暗恨怪獣 ヂリバー Gloomy Grudge Kaiju, Diriver"
                    => new[] { (Serial: "SSSS/001B-083", Rarity: "C", Name: new MultiLanguageString { EN = "Gloomy Grudge Kaiju, Diriver", JP = "幽愁暗恨怪獣 ヂリバー" }) },
                "SSSS/001T-080 C 多事多難怪獣 ゴーヤベック Kaiju of Many Difficulties, Go’yavec"
                    => new[] { (Serial: "SSSS/001B-080", Rarity: "C", Name: new MultiLanguageString { EN = "Kaiju of Many Difficulties, Go’yavec", JP = "多事多難怪獣 ゴーヤベック" }) },
                "HG/001B-102 Re 励ましの言葉 Words of Encouragement"
                    => new[] { (Serial: "HG/001B-102SP", Rarity: "ReSP", Name: new MultiLanguageString { EN = "Words of Encouragement", JP = "励ましの言葉" }) },
                "GGZ/001B-040 R リーナ・バーン Lina Byrne"
                    => new[] { (Serial: "GZ/001B-040", Rarity: "R", Name: new MultiLanguageString { EN = "Lina Byrne", JP = "リーナ・バーン" }) },
                // Exception due to Sleeves Slideshow in Touhou EX
                "Source" => new (string Serial, string Rarity, MultiLanguageString Name)[] { },
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
