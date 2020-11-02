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
        private readonly Regex effectMatcher = new Regex(@"(【(スパーク|のびしろ|ブロッカー|キャンセル|起|永|自)([^】]*)】：?)(.*)((\n[^【](.*))*)");

        public int Priority => 1;

        public bool IsCompatible(List<R4UCard> cards)
        {
            if (cards.First().Language != CardLanguage.Japanese)
                return false;
            else
                return true;
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
            var document = await url.WithReferrer("https://rebirth-fy.com/cardlist/").GetHTMLAsync();
            var flavorJPText = document.QuerySelector(".cardlist-flavor").GetInnerText();
            var rulesTextJPText = document.QuerySelector(".cardlist-free").GetInnerText().Trim();
            var imageLink = document.QuerySelector(".cardlist-img").FindChild<IHtmlImageElement>().Source;
            var rulesTextEnumerable = effectMatcher.Matches(rulesTextJPText);
            Log.Information("Flavor JP: {jp}", flavorJPText);
            Log.Information("Rules Text JP: {jp}", rulesTextJPText);
            if (!String.IsNullOrWhiteSpace(flavorJPText))
            {
                updatedCard.Flavor ??= new MultiLanguageString();
                updatedCard.Flavor.JP = flavorJPText;
            }
            updatedCard.Effect = rulesTextEnumerable.Select((m, i) =>
            {
                var result = card.Effect[i].Clone();
                result.JP = m.Value;
                return result;
            }).ToArray();
            updatedCard.Images.Add(new Uri(imageLink));
            Log.Information("After editing: {@card}", updatedCard);
            return updatedCard;
        }

        private bool HasMissingInformation(R4UCard card)
        {
            return (card?.Effect?.Any(mls => mls.JP == null) ?? false) || (card?.Name.JP == null) || (card?.Traits?.Any(mls => mls.JP == null) ?? false) || ((card.Images?.Count ?? 0) < 1);
        }
    }
}
