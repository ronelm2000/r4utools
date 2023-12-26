using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Flurl.Http;
using Flurl.Util;
using Montage;
using Montage.RebirthForYou;
using Montage.RebirthForYou.Tools;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Octokit;
using Serilog;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards;

public class R4UWebsiteRawParser : ICardSetParser
{
    private readonly ILogger Log = Serilog.Log.ForContext<R4UWebsiteRawParser>();
    private readonly string CardSearchExURL = "https://rebirth-fy.com/cardlist/cardsearch_ex";
    private readonly Regex releaseIDMatcher = new(@"(([A-Za-z0-9]+)(\/)([^-]+))-");
    private readonly Regex effectMatcher = new Regex(@"(【)(?(スパーク|起|永|自)(スパーク|起|永|自)(([^】]*)】：?)([^\n]*)((\n[^【](.*))*)|(キャンセル|ブロッカー|のびしろ)(】)(：)?([^\n]*)(\n)?(（(.+)）)?)");


    public bool IsCompatible(IParseInfo parseInfo)
    {
        return parseInfo.URI.StartsWith("https://rebirth-fy.com/");
    }

    public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
    {
        var cardEntries = GetQueryResponses(urlOrLocalFile).SelectMany(this.GetCardEntries).Select(this.GetCardFromEntry);
        await foreach (var card in cardEntries)
        {
            yield return card;
        }

        Log.Information("Done");
    }

    private async IAsyncEnumerable<IDocument> GetQueryResponses(string urlOrLocalFile)
    {
        int page = 1;
        int status = 200;
        string expansion = Flurl.Url.ParseQueryParams(urlOrLocalFile)[0].Value?.ToString().Trim();
        while ((await Flurl.Url.Parse(CardSearchExURL)
            .SetQueryParam("page", page)
            .SetQueryParam("view", "text")
            .SetQueryParam("parallel", 1)
            .SetQueryParam("expansion", expansion)
            .WithHeaders(new
            {
                Host = "rebirth-fy.com",
                Accept = "*/*",
                X_Requested_With = "XMLHttpRequest"
            })
            .WithAutoRedirect(false)
            .GetAsync()) is IFlurlResponse response
            && response.ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK
            )
        {
            yield return await response.ResponseMessage.RecieveHTML();
            page++;
        }
    }

    private IAsyncEnumerable<IHtmlAnchorElement> GetCardEntries(IDocument document)
        => document.QuerySelectorAll<IHtmlAnchorElement>(".cardlist-item").ToAsyncEnumerable();

    private R4UCard GetCardFromEntry(IHtmlAnchorElement htmlAnchorElement)
    {
        // var 
        Log.Information("Parsing: {html}", htmlAnchorElement.InnerHtml.TakeFirst(200));

        var entryDTs = htmlAnchorElement.QuerySelectorAll("dt");

        R4UCard card = new R4UCard();
        card.Serial = htmlAnchorElement.QuerySelector(".cardlist-number").GetInnerText().Trim();
        card.Set = new R4UReleaseSet { ReleaseCode = releaseIDMatcher.Match(card.Serial).Groups[1].Value };
        card.Name = new MultiLanguageString { JP = htmlAnchorElement.QuerySelector(".cardlist-title").GetInnerText().Trim() };

        card.Rarity = entryDTs.Where(dt => dt.InnerHtml == "レアリティ").First().NextElementSibling.TextContent.Trim();
        card.Type = ParseFromJPString(entryDTs.Where(dt => dt.InnerHtml == "カード種類").First().NextElementSibling.TextContent.Trim());
        card.Flavor = new MultiLanguageString { JP = htmlAnchorElement.QuerySelector(".cardlist-flavor").GetInnerText().Trim() };

        if (card.Type == CardType.Character)
        {
            card.Cost = int.Parse(entryDTs.Where(dt => dt.InnerHtml == "コスト").First().NextElementSibling.TextContent.Trim());
            card.ATK = int.Parse(entryDTs.Where(dt => dt.InnerHtml == "ATK").First().NextElementSibling.TextContent.Trim());
            card.DEF = int.Parse(entryDTs.Where(dt => dt.InnerHtml == "DEF").First().NextElementSibling.TextContent.Trim());
            card.Traits = entryDTs.Where(dt => dt.InnerHtml == "属性").First().NextElementSibling.TextContent
                .Split("・")
                .Select(t => new MultiLanguageString { JP = t.Trim() })
                .ToList();
        }

        if (card.Type != CardType.Partner)
        {
            var rulesTextJPText = htmlAnchorElement.QuerySelector(".cardlist-free").GetInnerText().Trim();
            card.Effect = effectMatcher.Matches(rulesTextJPText).Select(m => new MultiLanguageString { JP = m.Value.Trim() }).ToArray();
            card.Color = this.InferColorFromEffect(card.Effect);
        } else
        {
            card.Color = CardColor.Red;
        }

        Log.Information("Parsed: {cardSerial}", card.Serial);
        
        return card;
    }

    private CardType ParseFromJPString(string jpString)
    {
        return jpString.Trim() switch
        {
            "Reバース" => CardType.Rebirth,
            "パートナー" => CardType.Partner,
            _ => CardType.Character
        };
    }

    private CardColor InferColorFromEffect(MultiLanguageString[] effect)
    {
        if (effect.Any(e => e.JP.StartsWith("【スパーク】")))
            return CardColor.Yellow;
        else if (effect.Any(e => e.JP.StartsWith("【キャンセル】")))
            return CardColor.Green;
        else if (effect.Any(e => e.JP.StartsWith("【ブロッカー】")))
            return CardColor.Green;
        else
            return CardColor.Blue;
    }

}
