﻿using AngleSharp.Html.Dom;
using Flurl;
using Flurl.Http;
using JasperFx.Core;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards;
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
    public class RebirthFoilPostProcessor : ICardPostProcessor, ISkippable<ICardSetParser>
    {
        readonly Regex keywordSearchMatcher = new(@"(?:(.+?(?=\[(?:(?:.*))\]))(?:\[(?:(?:.*))\]))|(.+)");
        readonly string foilSearchURL = "https://rebirth-fy.com/cardlist/cardsearch?keyword=IMC%2F001T-001&keyword_type[]=no&search_type[]=or&expansion=&title=&card_kind=&cost_s=&cost_e=&atk_s=&atk_e=&def_s=&def_e=";
        public int Priority => 2;

        public ILogger Log = Serilog.Log.Logger.ForContext<RebirthFoilPostProcessor>();

        public Task<bool> IsCompatible(List<R4UCard> cards) => Task.FromResult(true);

        public async Task<bool> IsIncluded(ICardSetParser parser)
        {
            await Task.CompletedTask;
            return parser is R4URenegadesSetParser || parser is CSVPartnerOnlyParser || parser is R4UWebsiteRawParser;
        }

        public async IAsyncEnumerable<R4UCard> Process(IAsyncEnumerable<R4UCard> originalCards)
        {
            await foreach (var card in originalCards)
            {
                yield return card;
                Log.Information("Getting possible foils for [{serial}]", card.Serial);

                var keywordMatch = keywordSearchMatcher.Match(card.Serial);
                var keywordToUse = keywordMatch.Groups[1]?.Value;
                if (keywordToUse?.IsEmpty() ?? true)
                    keywordToUse = keywordMatch.Groups[2]?.Value;

                var urlRequest = new FlurlRequest(foilSearchURL).SetQueryParam("keyword", keywordToUse);
                Log.Debug("URL: {url}", urlRequest.Url);
                var doc = await urlRequest.GetHTMLAsync();
                var cardList = doc.QuerySelectorAll(".cardlist-item")
                    .Select(i => i as IHtmlAnchorElement)
                    .Where(i => new Url(i.Href).QueryParams[0].Value.ToString() != card.Serial);
                foreach (var cardLink in cardList)
                {
                    Log.Information("Found URL: {url}", cardLink.Href);
                    var cardLinkDoc = await cardLink.Href.WithReferrer(urlRequest.Url.Path).GetHTMLAsync();
                    var newCard = card.Clone();
                    newCard.NonFoil = card;
                    newCard.Serial = cardLinkDoc.QuerySelector(".cardlist-number").TextContent;
                    newCard.Rarity = cardLinkDoc.QuerySelectorAll(".cardlist-text") //
                        .Where(i => i.Children.ElementAt(2)?.TextContent == "レアリティ")
                        .Select(i => i.Children.ElementAt(3).TextContent.Trim())
                        .First();
                    var flavorJPText = cardLinkDoc.QuerySelector(".cardlist-flavor").TextContent;
                    if (!string.IsNullOrWhiteSpace(flavorJPText) && flavorJPText != "（無し）")
                    {
                        newCard.Flavor = new MultiLanguageString
                        {
                            JP = cardLinkDoc.QuerySelector(".cardlist-flavor").TextContent
                        };
                    }
                    yield return newCard;
                }
            }
        }
    }
}
