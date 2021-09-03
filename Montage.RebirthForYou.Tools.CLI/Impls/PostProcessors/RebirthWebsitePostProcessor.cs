using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Flurl;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.PostProcessors
{
    public class RebirthWebsitePostProcessor : ICardPostProcessor
    {
        private readonly ILogger Log = Serilog.Log.ForContext<RebirthWebsitePostProcessor>();
        private readonly string rebirthURLPrefix = "https://rebirth-fy.com/cardlist/";
        private readonly Regex effectMatcher = new Regex(@"(【)(?(スパーク|起|永|自)(スパーク|起|永|自)(([^】]*)】：?)([^\n]*)((\n[^【](.*))*)|(キャンセル|ブロッカー|のびしろ)(】)(：)?([^\n]*)(\n)?(（(.+)）)?)");

        public int Priority => 1;

        public async Task<bool> IsCompatible(List<R4UCard> cards)
        {
            await Task.CompletedTask;
            try
            {
                if (cards.First().Language != CardLanguage.Japanese)
                    return false;
                else
                    return true;
            } catch (InvalidOperationException)
            {
                return false;
            }
        }

        public async IAsyncEnumerable<R4UCard> Process(IAsyncEnumerable<R4UCard> originalCards)
        {
            Log.Information("Starting...");
            await foreach (var card in originalCards)
            {
                yield return await Process(card);
            }
        }

        private async Task<R4UCard> Process(R4UCard card)
        {
            if (!HasMissingInformation(card)) return card;
            var updatedCard = card.Clone();
            var url = rebirthURLPrefix
                .SetQueryParam("cardno", updatedCard.Serial.Replace("+", "＋")) //
                ;
            Log.Debug("Opening Link: {url}", url);
            try
            {
                var document = await url.WithReferrer("https://rebirth-fy.com/cardlist/").GetHTMLAsync();
               var nameJPText = document.QuerySelector(".cardlist-title").GetInnerText();
                var flavorJPText = document.QuerySelector(".cardlist-flavor").GetInnerText();
                var rulesTextJPText = GetWebsiteErrata(card) ?? document.QuerySelector(".cardlist-free").GetInnerText().Trim();
                var imageLink = document.QuerySelector(".cardlist-img").FindChild<IHtmlImageElement>().Source;
                var rulesTextEnumerable = effectMatcher.Matches(rulesTextJPText);
                Log.Information("Name JP: {jp}", nameJPText);
                Log.Information("Flavor JP: {jp}", flavorJPText);
                Log.Information("Rules Text JP: {jp}", rulesTextJPText.Substring(0, Math.Min(rulesTextJPText.Length, 50)));
                if (!String.IsNullOrWhiteSpace(flavorJPText) && flavorJPText != "（無し）")
                {
                    updatedCard.Name ??= new MultiLanguageString();
                    updatedCard.Name.JP = nameJPText;
                }

                if (!String.IsNullOrWhiteSpace(flavorJPText))
                {
                    updatedCard.Flavor ??= new MultiLanguageString();
                    updatedCard.Flavor.JP = flavorJPText;
                }
                if ((updatedCard.Effect?.Length ?? 0) != (rulesTextEnumerable?.Count ?? 0))
                    throw new FormatException($"Effect Text in EN for {card.Serial} does not match Effect Text in JP. May need to data loss!");

                updatedCard.Effect = rulesTextEnumerable.Select((m, i) =>
                {
                    var result = card.Effect[i].Clone();
                    result.JP = m.Value;
                    return result;
                }).ToArray();
                updatedCard.Images.Add(new Uri(imageLink));

                var jpTraits = document.QuerySelectorAll(".cardlist-text") //
                    .Where(i => i.Children.ElementAt(2)?.TextContent == "属性")
                    .Select(i => i.Children.ElementAt(3).TextContent.Trim())
                    .FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(jpTraits))
                {
                    updatedCard.Traits = updatedCard.Traits.Zip(jpTraits.Split("・"), (mls, jpTrait) => ModifiedIfJapaneseIsNull(mls, jpTrait)).ToList();
                }
                //Log.Information("After editing: {@card}", updatedCard);
                return updatedCard;
            }
            catch (Exception e) when (_exceptions.TryGetValue(card.Serial, out JPTextRecord exceptionalRecord))
            {
                Log.Information("Handling exceptional record: {serial}", card.Serial);
                updatedCard.Name.JP = exceptionalRecord.Name;
                updatedCard.Rarity = exceptionalRecord.Rarity;
                updatedCard.Effect = (from eff in updatedCard.Effect.Zip(exceptionalRecord.Rules)
                                      select new MultiLanguageString { EN = eff.First.EN, JP = eff.Second }).ToArray();
                updatedCard.Traits = (from trait in updatedCard.Traits.Zip(exceptionalRecord.Traits)
                                      select new MultiLanguageString { EN = trait.First.EN, JP = trait.Second }).ToList();
                updatedCard.Flavor ??= new MultiLanguageString();
                updatedCard.Flavor.JP = exceptionalRecord.Flavor;
                updatedCard.Images ??= new List<Uri>();
                updatedCard.Images.Add(new Uri(exceptionalRecord.ImageLink));
                return updatedCard;
            }
        }

        private string GetWebsiteErrata(R4UCard card)
        {
            return (card.NonFoil?.Serial ?? card.Serial) switch
            {
                "HG/001B-045" => "【自】【メンバー】：このキャラがサポートした時、あなたはエネ②することで、１枚引き、このアタック中、サポートされたキャラを＋１/±０。",
                "HP/001B-093" => "【自】【メンバー】：このキャラがサポートした時、あなたはエネ②することで、１枚引き、このアタック中、サポートされたキャラを＋１/±０。",
                _ => null
            };
        }

        private MultiLanguageString ModifiedIfJapaneseIsNull(MultiLanguageString mls, string jpText)
        {
            if (mls?.JP != null) return mls;
            else return new MultiLanguageString { EN = mls.EN, JP = jpText };
        }

        private bool HasMissingInformation(R4UCard card)
        {
            return (card?.Effect?.Any(mls => mls.JP == null) ?? false) || (card?.Name.JP == null) || (card?.Traits?.Any(mls => mls.JP == null) ?? false) || ((card.Images?.Count ?? 0) < 1);
        }

        private Dictionary<string, JPTextRecord> _exceptions = new Dictionary<string, JPTextRecord>()
        {
            ["HG/001B-102"] = ( name: "励ましの言葉",
                                rarity: "Re",
                                flavor: null,
                                rules: new string[] { "【永】：あなたのエントリーを＋２/＋３。" },
                                traits: new string[] { },
                                imageLink: "https://s3-ap-northeast-1.amazonaws.com/rebirth-fy.com/wordpress/wp-content/images/cardlist/HGBP/hg001b-102sp.png"
                                ),
            ["HG/001B-103"] = (name: "カケラの鑑賞者",
                                rarity: "Re",
                                flavor: null,
                                rules: new string[] { "【スパーク】：あなたは１点回復する。", "【永】：各ターン１回目のアタック中、あなたのエントリーを＋１/±０。" },
                                traits: new string[] { },
                                imageLink: "https://s3-ap-northeast-1.amazonaws.com/rebirth-fy.com/wordpress/wp-content/images/cardlist/HGBP/hg001b-103sp.png"
                                )
        };
    }

    internal class JPTextRecord
    {
        internal string Flavor { get; set; }
        internal string Name { get; set; }
        internal string Rarity { get; set; }
        internal string[] Rules { get; set; }
        internal string[] Traits { get; set; }
        internal string ImageLink { get; set; }

        public JPTextRecord(string name, string rarity, string flavor, string[] rules, string[] traits, string imageLink) => 
            (Name, Rarity, Flavor, Rules, Traits, ImageLink) = (name, rarity, flavor, rules, traits, imageLink);

        public static implicit operator JPTextRecord((string name, string rarity, string flavor, string[] rules, string[] traits, string imageLink) tuple) =>
            new JPTextRecord(tuple.name, tuple.rarity, tuple.flavor, tuple.rules, tuple.traits, tuple.imageLink);

    }
}
