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
            }
            catch (InvalidOperationException)
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
            var url = GetSerialPageExceptions(card);
            Log.Debug("Opening Link: {url}", url);
            try
            {
                var document = await url.WithReferrer("https://rebirth-fy.com/cardlist/").GetHTMLAsync();
                var nameJPText = document.QuerySelector(".cardlist-title").GetInnerText();
                var flavorJPText = document.QuerySelector(".cardlist-flavor").GetInnerText();
                var rulesTextJPText = GetWebsiteErrata(card) ?? document.QuerySelector(".cardlist-free").GetInnerText().Trim();
                var rarityText = document.QuerySelectorAll(".cardlist-text") //
                    .Where(i => i.Children.ElementAt(2)?.TextContent == "レアリティ")
                    .Select(i => i.Children.ElementAt(3).TextContent.Trim())
                    .First();
                var imageLink = document.QuerySelector(".cardlist-img").FindChild<IHtmlImageElement>().Source;
                var rulesTextEnumerable = effectMatcher.Matches(rulesTextJPText);
                Log.Information("Name JP: {jp}", nameJPText);
                Log.Information("Flavor JP: {jp}", flavorJPText);
                Log.Information("Rules Text JP: {jp}", rulesTextJPText.Substring(0, Math.Min(rulesTextJPText.Length, 50)));
                if (!String.IsNullOrWhiteSpace(nameJPText) && nameJPText != "（無し）")
                {
                    updatedCard.Name ??= new MultiLanguageString();
                    updatedCard.Name.JP = nameJPText;
                }

                if (!String.IsNullOrWhiteSpace(flavorJPText))
                {
                    updatedCard.Flavor ??= new MultiLanguageString();
                    updatedCard.Flavor.JP = flavorJPText;
                }

                if (!String.IsNullOrWhiteSpace(rarityText))
                {
                    updatedCard.Rarity = rarityText;
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
            catch (Exception) when (_exceptions.TryGetValue(card.Serial, out JPTextRecord exceptionalRecord))
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

        private Flurl.Url GetSerialPageExceptions(R4UCard card)
        {
            return card switch
            {
                R4UCard c when card.Serial.StartsWith("KS/002T-") && card.Type != CardType.Partner
                    => rebirthURLPrefix.SetQueryParam("cardno", card.Serial.Replace("+", "＋").Replace("KS/002T-", "KS/002-T")),
                _ => rebirthURLPrefix.SetQueryParam("cardno", card.Serial.Replace("+", "＋"))
            };
        }

        private string GetWebsiteErrata(R4UCard card)
        {
            return (card.NonFoil?.Serial ?? card.Serial) switch
            {
                "TH/001B-017" => "【自】【エントリー】：このキャラが相手のキャラをリタイアさせた時、あなたの控え室にカードが７種類以上あるなら、あなたは自分の控え室からキャラを１枚選び、手札に戻す。（別名のみカウントする）",
                "TH/001B-014" => "【自】【エントリー】【本領発揮Lv３】〔紅魔郷〕：このキャラがアタックした時、あなたはエネ②することで、このアタック中、このキャラを＋２/±０。",
                "HG/001B-045" => "【自】【メンバー】：このキャラがサポートした時、あなたはエネ②することで、１枚引き、このアタック中、サポートされたキャラを＋１/±０。",
                "HP/001B-093" => "【自】【メンバー】：このキャラがサポートした時、あなたはエネ②することで、１枚引き、このアタック中、サポートされたキャラを＋１/±０。",
                "HP/002E-024" => "【スパーク】：あなたは１点回復する。\n【永】：各ターン１回目のアタック中、あなたのエントリーを＋１/±０。",
                "GP/001C-001" => "【スパーク】【本領発揮Lv５】〔Poppin'Party〕：あなたは相手の【レスト】しているメンバーから２枚選び、控え室に置く。\n【起】【エントリー】：［エネ④］あなたは１枚引く。",
                "LR/001B-004" => "【スパーク】【本領発揮Lv５】：あなたは自分の控え室から１枚選び、エネルギーに【レスト】で置く。あなたのメンバーに《たきな》がいるなら、控え室ではなくデッキの上から置いてよい。\n【永】【エントリー】【Reコンボ】《今日から相棒》【本領発揮Lv５】：このキャラを＋２/±０。",
                "RE/001E-025" => "【ブロッカー】：【のびしろ】or〔twinkle♡way〕\n【自】：このキャラがブロックしたアタック終了時、あなたは自分の、エントリーとリタイアから【のびしろ】を１枚ずつ選び、それらを入れ替えてよい。",
                "KGJT/001B-015" => "【自】【メンバー】：このキャラが、《シャドウ》か裏向きのキャラをサポートした時、あなたはこのキャラをデッキの下に置くことで、自分の控え室からキャラを１枚選び、手札に戻す。\n【起】【エントリー】：［エネ①］あなたはこのキャラをコスト５でATK５/DEF９の『ベータ』のキャラとして裏向きにし、このターン終了時、表向きにする。",
                "KGJT/001B-022" => "【自】【メンバー】【本領発揮Lv４】：このキャラが、《シャドウ》か裏向きのキャラをサポートした時、あなたはこのキャラをエネルギーに【レスト】で置くことで、１枚引く。\n【起】【エントリー】：［エネ②］あなたはこのキャラをコスト４でATK５/DEF８の『ガンマ』のキャラとして裏向きにし、このターン終了時、表向きにする。",
                "KGJT/001B-036" => "【自】【メンバー】【本領発揮Lv４】：このキャラが、《シャドウ》か裏向きのキャラをサポートした時、このアタック中、サポートされたキャラを＋２/±０。\n【起】【エントリー】：［エネ①］あなたはこのキャラをコスト５でATK５/DEF９の『イプシロン』のキャラとして裏向きにし、このターン終了時、表向きにする。",
                "LR/001E-030" => "【ブロッカー】《千束》or《たきな》\n【自】【本領発揮Lv５】：このReバースがブロックした時、あなたはこのReバースをセットし、１枚引く。\n【永】：あなたのエントリーを＋１/±０。",
                "KS/002T-005" => "【永】【エントリー】【Reコンボ】：このキャラを＋２/±０。 （【Reコンボ】：あなたのReバースがセットしてあれば有効）\n【自】：このキャラがエネルギーから控え室に置かれた時、あなたは自分のエントリーから《めぐみん》を１枚選び、このターン中、そのキャラを＋３/±０。",
                "KS/002B-018" => "【永】【エントリー】：あなたのメンバーがいないなら、このキャラを±０/＋４。\n【自】【メンバー】：あなたのターン終了時、あなたは自分の他のメンバーから１枚以上、すべて選び、控え室に置くことで、このキャラをエントリーに置く。",
                "KS/002B-093" => "【スパーク】：あなたは味方のメンバーから２枚まで選び、控え室に置く。\n【自】：このReバースがセットされた時、あなたのリタイアが３枚以上なら、あなたは相手の、エントリーかメンバーから１枚選び、控え室に置く。",
                "KS/002B-096" => "【スパーク】：あなたは自分のメンバーから１枚選び、手札に戻してよい。\n【起】【ターン１】：あなたのパートナーをすべて【スタンド】する。",
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
