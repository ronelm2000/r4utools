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
using System.Reflection.Metadata;
using AngleSharp.Common;
using CommandLine;
using System.Reflection.Metadata.Ecma335;
using System.ComponentModel;
using System.Reactive.PlatformServices;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class R4URenegadesSetParser : ICardSetParser
    {
        private readonly ILogger Log = Serilog.Log.ForContext<R4URenegadesSetParser>();

        private readonly Regex serialRarityJPNameMatcher = new(@"([^ ]+) ([A-Za-z0-9]+) (.*?(?=(?: ?)<strong>))(?:<strong>)(.+?(?=<br><\/strong>|<\/strong><br>|<\/strong>|<\/strong>&ZeroWidthSpace;<br>$))");
        private readonly Regex serialTrialJPNameMatcher = new(@"((?:\w)+\/(?:\d)+T-(?:\d)+) (.*)(?:(?: ?)<strong>)(.+)(?:<br><\/strong>|<\/strong><br>)");
        private readonly Regex costSeriesTraitMatcher = new(@"(?:Cost )([0-9]+)(?: \/ )(.+)(?: \/ )(.+)");
        private readonly Regex seriesRebirthMatcher = new(@"(.+)(?: )*(\/|\\)(?: )*(.+) Rebirth");
        private readonly Regex rubyMatcher = new(@"(<rt>)([^>]+)(<\/rt>)|(<ruby>)|(<\/ruby>)");
        private readonly Regex releaseIDMatcher = new(@"(([A-Za-z0-9]+)(\/)([^-]+))-");
        private readonly Regex overflowEffectTextMatcher = new(@"^(?=(\()|i\.|ii\.|iii\.|iv\.|(\d+) or more:|(One|Two|Three|Four|Five|Six|Seven) or more:|(1st|2nd|3rd|4th|5th|1st and 2nd) time:|\[(Spark|Cancel|Blocker)\]\.).+");

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
            var setMap = new Dictionary<string, R4UReleaseSet>();
            return PatchExceptionalListFormats(document).SelectMany(f => CreateBaseCards(f, setMap).ToAsyncEnumerable());
        }

        private IAsyncEnumerable<R4UCard> ParseTagPage(IDocument document)
        {
            Log.Information("Parsing as Tag page...");
            var setMap = new Dictionary<string, R4UReleaseSet>();
            var querySelector = document.QuerySelectorAll(".tag-cotd").AsEnumerable()
                .Select(a => a.QuerySelector<IHtmlAnchorElement>(".post-title > a").Href)
                .ToAsyncEnumerable()
                .SelectAwait(async a => await new Uri(a).DownloadHTML(("Referer", "rebirthforyourenegades.wordpress.com")).WithRetries(10))
                .SelectMany(PatchExceptionalListFormats);

            return querySelector.SelectMany(f => CreateBaseCards(f, setMap).ToAsyncEnumerable());
        }

        private IAsyncEnumerable<IElement> PatchExceptionalListFormats(IDocument document)
        {
            Log.Information("Computing for all elements to parse: {}", document.DocumentUri.ToString());
            var postContent = document.QuerySelector(".post-content");
            var result = document.DocumentUri switch
            {
                // Alot of images for this site are missing and coalesced into a list of <p> tags
                "https://rebirthforyourenegades.wordpress.com/2022/12/02/hp-007t-hololive-5th-gen-full-list/"
                    => postContent.QuerySelectorAll(".wp-block-image").AsEnumerable()
                        .Concat(postContent.QuerySelectorAll("p")
                            .SkipWhile(e => e.InnerHtml.StartsWith("The full list, "))
                        ),

                // Default
                _ => postContent.QuerySelectorAll(".wp-block-image").AsEnumerable()
                    .Concat(postContent.QuerySelectorAll(".wp-block-jetpack-slideshow"))
                    .Concat(postContent.QuerySelectorAll(".wp-block-group"))
            };
            return result.ToAsyncEnumerable();
        }

        private IEnumerable<R4UCard> CreateBaseCards(IElement figureOrImage, Dictionary<string, R4UReleaseSet> setMap)
        {
            var nextElementSibling = figureOrImage.NextElementSibling;
            if (nextElementSibling == null)
            {
                Log.Information("There seems to be nothing after this figure; ignoring and returning no cards.");
                return Enumerable.Empty<R4UCard>();
            }
            if (nextElementSibling.TagName.ToLower() == "figure")
            {
                Log.Information("Another Figure is right after this Figure; we'll skip it and check for that figure instead.");
                return Enumerable.Empty<R4UCard>();
            }
            if (nextElementSibling.ClassList.Contains("wp-block-group"))
            {
                Log.Information("Newer sets (starting from LycoReco) use a list of wp-block-groups; Skipping and Checking the Next Div.");
                return Enumerable.Empty<R4UCard>();
            }
            if (figureOrImage.NextElementSibling is IHtmlSpanElement)
            {
                Log.Information("Under certain cirmstances <p> is used as a precursor to a set; <span> afterwards mean it's the ad html block. Skipping.");
                return Enumerable.Empty<R4UCard>();
            }
            if (figureOrImage.NextElementSibling is IHtmlParagraphElement paragraph)
            {
                return CreateBaseCards(paragraph, setMap);
            }
            throw new NotImplementedException("There should have been a <p> tag after the <figure> tag, but instead found nothing.");
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
                var firstLineMatch = PatchExceptionalSerialRarityMatches(serialRarityJPNameMatcher.Match(content));
                card.Serial = firstLineMatch.Serial;
                card.Rarity = firstLineMatch.Rarity;
                card.Name = new MultiLanguageString
                {
                    JP = firstLineMatch.NameJP,
                    EN = firstLineMatch.NameEN
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

                Regex flavorTextMatcher = new(@"" + Regex.Escape(rebirthLine) + @"<br><em>(.+)</em><br>");
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

                Regex flavorTextMatcher = new(@"" + defLine + @"<br><em>(.+)(?:</em><br>|<br></em>)");
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

            if (TryGetErrata(card) is RenegadesCharacterPatch errataEntry)
            {
                (   int? ATK,
                    int? DEF, 
                    int? Cost,
                    MultiLanguageString[]? Traits,
                    MultiLanguageString? FlavorText,
                    MultiLanguageString[]? Effects
                    ) = errataEntry;
                card.Type = CardType.Character;
                card.Color = CardUtils.InferFromEffect(Effects ?? card.Effect);
                card.ATK = ATK ?? card.ATK;
                card.DEF = DEF ?? card.DEF;
                card.Cost = Cost ?? card.Cost;
                card.Traits = Traits?.ToList() ?? card.Traits;
                card.Flavor = FlavorText ?? card.Flavor;
                card.Effect = Effects ?? card.Effect;
            }

            if (card.Serial == null)
                throw new NullReferenceException("Serial cannot be null; there must be a parsing error somewhere.");

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
                "Cost4C / BanG Dream! Girls Band Party!☆PICO Fever! / Music – Afterglow" => (
                    Cost: 4,
                    Type: CardType.Character,
                    Traits: new[] { "Music", "Afterglow" }
                ),
                _ => null
            };
            return errata != null;
        }

        private RenegadesCharacterPatch? TryGetErrata(R4UCard card)
        {
            return card.Serial switch
            {
                "KGND/001B-079[VA]" => new RenegadesCharacterPatch {
                    ATK = 6,
                    DEF = 7,
                    FlavorText = new MultiLanguageString { EN = "It seems like my twin sister has a deep dark side." },
                    Effects = new[] { new MultiLanguageString() { EN = "[Growing](Expect growth in the future!)" } }
                },

                "YC/001B-046" => new RenegadesCharacterPatch
                {
                    ATK = 4,
                    DEF = 5,
                    FlavorText = new MultiLanguageString { EN = "It costs about 2 to 3 thousand more than synthetic fibers at the same cold-resistant temperature..." },
                    Effects = new[] { new MultiLanguageString { EN = "[Spark]: Perform all of the following based on the characters on your member area. \"Nadeshiko\": Draw a card, choose a card from your hand, and put it into the waiting room. \"Chiaki\": Choose a character from your waiting room, and you may put it onto an open member area.\r\n“Nadeshiko” and “Chiaki“: This character gets +3/+3 until end of turn." } }
                },
                "YC/001B-075" => new RenegadesCharacterPatch
                {
                    ATK = 2,
                    DEF = 3,
                    FlavorText = new MultiLanguageString { EN = "I'll be there soon." },
                    Effects = new[]
                    {
                        new MultiLanguageString
                        {
                            EN = "[Blocker \"Nadeshiko\"]"
                        },
                        new MultiLanguageString
                        {
                            EN = "[AUTO] When this character blocks, you may place this card into your vacant member area."
                        }
                    }
                },

                "HS/001B-049" => new RenegadesCharacterPatch
                {
                    ATK = 3,
                    DEF = 5,
                    FlavorText = new MultiLanguageString { EN = "Hehe, shopping together like this... somehow feels like we're newlyweds, doesn't it?" },
                    Effects = new[]
                    {
                        new MultiLanguageString
                        {
                            EN = "[Blocker [Growing]]"
                        },
                        new MultiLanguageString
                        {
                            EN = "[Cancel]:[Spark] of non-Rebirth cards."
                        },
                        new MultiLanguageString
                        {
                            EN = "[CONT]:This character’s [Cancel] can only be used when you have a [Growing] entry."
                        }
                    }
                },

                "TOP/001B-024" => new RenegadesCharacterPatch
                {
                    ATK = 4,
                    DEF = 6,
                    Cost = 3,
                    FlavorText = (EN: "Welcome~", JP: null),
                    Traits = new MultiLanguageString[]
                    {
                        (EN: "Comedy", JP: null),
                        (EN: "Dangan Kunoichi", JP: null)
                    },
                    Effects = new MultiLanguageString[]
                    {
                        (EN: "[Spark]: This character gets +3/±０ until end of turn.", JP: null)
                    }
                },

                "TS/001B-036" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (EN: "[Spark][Skill Showcase Level 3]:Choose up to 1 [Spark] of a card on your opponent’s retire area, and activate it as this character’s [Spark].", JP: null)
                    }
                },

                "GU/002B-068" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (   EN:  """
                                 [Spark]:Choose 1 between “Bread” and “Coffee“, and perform the following based on your choice.
                                 Bread. Choose a character from your waiting room, and put it onto your open member area as [Rest].
                                 Coffee. Your opponent chooses a character from their waiting room, and puts it onto their open member area as [Rest].
                                 """,
                            JP: null
                        )
                    }
                },

                "KS/002B-026" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (   EN: """
                                [ACT](Member)[ReCombo “Resolution to Battle“][1/Turn]:[Choose 2 or more of your partners, and [Rest] them], and perform all of the following based on the number of chosen cards.
                                2 or more: Choose 1 of your entries, and that character gets +3/±0 until end of turn.
                                3: Draw a card, and put the top card of your deck onto your energy area.
                                """,
                            JP: null
                        )
                    }
                },
                "KS/002B-027" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (   EN: """
                                [Spark]:Perform all of the following based on the characters on your member area.
                                “Megumin“: Draw a card、choose a card from your hand, and put it into the waiting room.
                                “Komekko“: Choose a character from your waiting room、and you may put it onto your open member area.
                                Both “Megumin“ and “Komekko“: This character gets +3/+3 until end of turn.
                                """,
                            JP: null
                        )
                    }
                },
                "KS/002B-070" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (   EN: """
                                [AUTO][Skill Showcase Level 4]:When this character is put onto your member area, choose 1 between “Dark God” and “Food“, and perform the following based on your choice. (Has priority over entry in)
                                Dark God: Choose 1 of your opponent’s [Rest] members, and put it into the waiting room.
                                Food: [Choose a card from your hand, and put it into the waiting room], and draw a card.
                                """,
                            JP: null
                        )
                    }
                },
                "KS/002B-091" => new RenegadesCharacterPatch
                {
                    FlavorText = (EN: "“Let the smoke from my explosion be a signal to the world’s mightiest!”", JP: null),
                    Effects = new MultiLanguageString[]
                    {
                        (EN: "[Spark]:[Choose a card from your hand, and put it into the waiting room], choose up to 2 cards from your waiting room, and put them onto your energy area.", JP: null),
                        (EN: "[AUTO][Skill Showcase Level 5]:When this Rebirth is set, [Cost (3)], choose the same number of your opponent’s members as cards in your retire area, and put them into the waiting room.", JP: null)
                    }
                },
                "KS/002B-093" => new RenegadesCharacterPatch
                {
                    Effects = new MultiLanguageString[]
                    {
                        (EN: "[Spark]:Choose up to 2 of your allied members, and put them into the waiting room.", JP: null),
                        (EN: "[AUTO] When this Rebirth is set, if you have 3 or more cards in your retire area, choose one of your opponent’s entries or members, and put it into the waiting room.", JP: null)
                    }
                },

               _ => null
            };
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


                // Exception due to having a/b in the same line.
                "KS/001B-096a/b Re 爆裂魔法 Explosion Magic"
                    => new[]
                    {
                        (Serial: "KS/001B-096a", Rarity: "Re", Name: new MultiLanguageString { EN = "Explosion Magic", JP = "爆裂魔法" }),
                        (Serial: "KS/001B-096b", Rarity: "Re", Name: new MultiLanguageString { EN = "Explosion Magic", JP = "爆裂魔法" })
                    },

                // Exception due to missing rarity
                "NJPW/001TV-034 リングイン 鈴木 みのる Ring In, Minoru Suzuki"
                    => new[] { (Serial: "NJPW/001TV-034", Rarity: "TD", Name: new MultiLanguageString { EN = "Ring In, Minoru Suzuki", JP = "リングイン 鈴木 みのる" }) },
                "NJPW/002B-P015 PP 内藤 哲也 Tetsuya Naito"
                    => new[]
                    {
                        (
                            Serial: "NJPW/002B-P015PP",
                            Rarity: "PP",
                            Name: new MultiLanguageString { EN = "Tetsuya Naito", JP = "内藤 哲也" }
                        )
                    },
                "NJPW/002B-P020 PP グレート-O-カーン Great-O-Khan"
                    => new[]
                    {
                        (
                            Serial: "NJPW/002B-P020PP",
                            Rarity: "PP",
                            Name: new MultiLanguageString { EN = "Great-O-Khan", JP = "グレート-O-カーン" }
                        )
                    },

                "YC/001B-026 日の出の時間 リン Time of Sunrise, Rin"
                    => new[]
                    {
                        (
                            Serial: "YC/001B-026",
                            Rarity: "C",
                            Name: new MultiLanguageString { EN = "Time of Sunrise, Rin", JP = "日の出の時間 リン" }
                        )
                    },
                "YC/001B-062 体験入隊 恵那 Trial Enlistment, Ena"
                    => new[]
                    {
                        (
                            Serial: "YC/001B-062",
                            Rarity: "R",
                            Name: new MultiLanguageString
                            {
                                EN = "Trial Enlistment, Ena",
                                JP = "体験入隊 恵那"
                            }
                        )
                    },

                // Exception due to a~q serial
                "KGND/001B-095a~q[VA] Re ――これは、小さな奇跡の物語 –This, is the story of a small miracle"
                    => Enumerable.Range('a', 17)
                        .Select(x => (char)x)
                        .Select(c => (Serial: $"KGND/001B-095{c}[VA]", Rarity: "Re", Name: new MultiLanguageString { EN = "–This, is the story of a small miracle", JP = "――これは、小さな奇跡の物語" }))
                        .ToArray(),
                "KGND/001B-072[VA] R 兄妹の信頼関係 ちはや Sibling’s Trust Relationship, Kyou"
                    => new[]
                    {
                        (
                            Serial: "KGND/001B-072[VA]",
                            Rarity: "R",
                            Name: new MultiLanguageString
                            {
                                EN = "Sibling’s Trust Relationship, Chihaya",
                                JP = "体験入隊 恵那"
                            }
                        )
                    },

                "LR/001T-017 TD 私はキミと会えて嬉しい！ I’m glad I was able to meet you"
                    => Enumerable.Range('a', 2)
                        .Select(x => (char)x)
                        .Select(c => (
                            Serial: $"LR/001T-017{c}", 
                            Rarity: "TD", 
                            Name: new MultiLanguageString { 
                                EN = "I’m glad I was able to meet you", 
                                JP = "私はキミと会えて嬉しい！"
                            }
                        ))
                        .ToArray(),

                "TOP/001B-024 Rarity JPName **ENName**"
                    => new[]
                    {
                        (
                            Serial: "TOP/001B-024",
                            Rarity: "C",
                            Name: new MultiLanguageString
                            {
                                EN = "Self-Defense Mindset, Kana",
                                JP = "自己防衛の心がけ かな"
                            }
                        )
                    },

                "Source" => new (string Serial, string Rarity, MultiLanguageString Name)[] { },
                _ => null
            };  
            return exceptionalResult != null;
        }

        private SerialRarityNameRow PatchExceptionalSerialRarityMatches(Match serialRarityNameMatch)
        {
            SerialRarityNameRow result = serialRarityNameMatch switch
            {
                Match m when m.Groups[1].Value.StartsWith("HS/001B-P") && (m.Groups[2] is { Value: "SNP" or "PP" }) =>
                    new (Serial: m.Groups[1].Value.Trim() + m.Groups[2].Value.Trim()),
                Match m when m.Groups[1].Value.StartsWith("KGND/001B-P") && !m.Groups[1].Value.EndsWith("[VA]") =>
                    new (Serial: m.Groups[1].Value.Trim() + "[VA]"),

                _ => new()
            };
            return result with
            {
                Serial = result.Serial ?? serialRarityNameMatch.Groups[1].Value.Trim(),
                Rarity = result.Rarity ?? serialRarityNameMatch.Groups[2].Value.Trim(),
                NameJP = result.NameJP ?? rubyMatcher.Replace(serialRarityNameMatch.Groups[3].Value, "").Trim(),
                NameEN = result.NameEN ?? serialRarityNameMatch.Groups[4].Value.Trim()
            };
        }

        private List<MultiLanguageString> Compress(List<MultiLanguageString> effects)
        {
            var overflowEffects = effects   .Select((mls, i) => (Effect: mls, Index: i)) //
                                            .Where((pair) => overflowEffectTextMatcher.IsMatch(pair.Effect.EN))
                                            .ToDictionary((pair) => pair.Index, (pair) => pair.Effect);
            List<MultiLanguageString> newEffects = effects.Select((mls, i) => (Effect: mls, Index: i)) //
                                                            .Where((pair) => !overflowEffectTextMatcher.IsMatch(pair.Effect.EN))
                                                            .Select((pair) => GroupConcat(pair, overflowEffects))
                                                            .Where((pair) => !string.IsNullOrEmpty(pair.EN))
                                                            .ToList();
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

    internal record SerialRarityNameRow (String Serial = null, String Rarity = null, String NameJP = null, String NameEN = null);

    internal record struct RenegadesCharacterPatch
    {
        internal int? ATK { get; init; }
        internal int? DEF { get; init; }
        internal int? Cost { get; init; }
        internal MultiLanguageString[]? Traits { get; init; }
        internal MultiLanguageString? FlavorText { get; init; }
        internal MultiLanguageString[]? Effects { get; init; }

        internal void Deconstruct(out int? ATK, out int? DEF, out int? Cost, out MultiLanguageString[] Traits, out MultiLanguageString FlavorText, out MultiLanguageString[] Effects)
        {
            ATK = this.ATK;
            DEF = this.DEF;
            Cost = this.Cost;
            Traits = this.Traits;
            FlavorText = this.FlavorText;
            Effects = this.Effects;
        }
    }
}
