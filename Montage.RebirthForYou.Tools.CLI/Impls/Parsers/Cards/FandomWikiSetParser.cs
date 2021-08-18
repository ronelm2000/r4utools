using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class FandomWikiSetParser : ICardSetParser
    {
        private Regex fandomMatcher = new Regex(@"(.*)://rebirth-for-you\.fandom\.com/wiki/(.*)");
        private Regex effectMatcher = new Regex(@"(\[(CONT|AUTO|ACT|Spark|Blocker|Cancel|Relaxing|Growing)([^\]]*)\])(.*)((\n[^\[](.*))*)");
        private Regex serialMatcher = new Regex(@"(?:- )?((\w+\/\w+)-\w*\d+[\w\+]{0,4}(?:\[[\w\\\/]+\])?)(?: )?\((\w*\+?)\)");
        private Regex releaseIDMatcher = new Regex(@"(?:- )?((\w+\/\w+))");
        private string[] nonFoilRarities = new string[] { "RRR", "RR", "R", "U", "C", "TD", "SD", "ReR", "ReC", "P", "PR", "BP" };
        private Func<CardDatabaseContext> _database;

        public ILogger Log { get; }

        public FandomWikiSetParser(IContainer ioc)
        {
            Log ??= Serilog.Log.ForContext<FandomWikiSetParser>();
            _database = () => ioc.GetService<CardDatabaseContext>();
        }

        public bool IsCompatible(IParseInfo parseInfo)
        {
            return fandomMatcher.IsMatch(parseInfo.URI);//.Contains("rebirth-for-you.fandom.com/wiki/");
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            Log.Information("Starting...");
            var document = await new Uri(urlOrLocalFile).DownloadHTML(("Referer", "rebirth-for-you.fandom.com")).WithRetries(10);
            var table = document.QuerySelector<IHtmlTableElement>(".set-table");
            var isPRPage = document.QuerySelectorAll(".portable-infobox").Count() < 1;
            var setData = (!isPRPage) ? CreateInfoBoxDataTable(document.QuerySelector(".portable-infobox")) : null;
            var prSetData = (isPRPage) ? CreatePRSets(table) : null;
            Log.Verbose("Set Data: @{data}", setData);
            Log.Verbose("PR Set Data: @{prSetData}", prSetData);

            var context = new WikiSetContext { SetData = setData, URL = urlOrLocalFile, Set = ParseSet(setData), Sets = prSetData };

            if (table?.Rows != null)
                foreach (var row in table.Rows)
                {
                    if (IsRowCompatible(row))
                        await foreach (var card in ParseCards(row, context))
                            yield return card;
                }
            Log.Information("Ending...");
            //throw new NotImplementedException();
            yield break;
        }

        private Dictionary<string, R4UReleaseSet> CreatePRSets(IHtmlTableElement table)
        {
            if (table != null)
            {
                return table.Rows //
                    .Select(row => releaseIDMatcher.Match(row.Cells[0].TextContent).Groups[1].Value) //
                    .Distinct() //
                    .ToDictionary(rid => rid, rid => CreatePromoSet(rid)) //
                    ;
            }
            else
                return new Dictionary<string, R4UReleaseSet>();
        }

        private R4UReleaseSet ParseSet(Dictionary<string, string> setData)
        {
            if (setData != null)
            {
                var res = new R4UReleaseSet();
                res.ReleaseCode = setData["prefix"];
                res.Name = setData["title"];
                return res;
            } else
            {
                return null;
            }
        }

        private R4UReleaseSet CreatePromoSet(string releaseID)
        {
            var res = new R4UReleaseSet();
            res.ReleaseCode = releaseID;
            res.Name = $"{releaseID} Promotional Cards";
            return res;
        }

        private Dictionary<string, string> CreateInfoBoxDataTable(IElement element)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            res["title"] = element.QuerySelector(".pi-title").TextContent;
            foreach (var elem in element.QuerySelectorAll<IHtmlDivElement>(".pi-item"))
                res[elem.QuerySelector(".pi-data-label").TextContent.ToLower()] = elem.QuerySelector(".pi-data-value").TextContent;
            return res;
        }

        private bool IsRowCompatible(IHtmlTableRowElement row)
        {
            return row.Cells[1].ChildNodes[0] is IHtmlAnchorElement anchor
                && anchor.Relation != "nofollow";
        }

        private async IAsyncEnumerable<R4UCard> ParseCards(IHtmlTableRowElement row, WikiSetContext context)
        {
            var cardLink = row.Cells[1].ChildNodes[0] as IHtmlAnchorElement;
            Log.Information("Following link: {link}", cardLink.Href);
            var cardDocument = await new Uri(cardLink.Href).DownloadHTML(("Referer", context.URL)).WithRetries(10);

            var rawMainInfoBox = cardDocument.QuerySelector<IHtmlTableElement>(".info-main > table") //
                    .Rows
                    .Select(x => (x.Cells[0].TextContent, x.Cells[1]))
                    .ToDictionary(p => p.Item1.Trim().ToLower(), p => p.Item2)
                    ;
            var mainInfoBox = rawMainInfoBox.ToDictionary(p => p.Key, p => p.Value.TextContent.Trim());

            var extraInfoBox = cardDocument.QuerySelector(".info-extra") //
                .ChildNodes //
                .Where(x => x is IHtmlTableElement) //
                .Select(x => x as IHtmlTableElement) //
                .Select(t => (t.Rows[0].Cells[0].TextContent, t.Rows[1].Cells[0].GetInnerText())) //
                .ToDictionary(p => p.Item1.Trim().ToLower(), p => p.Item2.Trim());

            var cardContext = new WikiCardContext { ExtraInfoBox = extraInfoBox, MainInfoBox = mainInfoBox, RawMainInfoBox = rawMainInfoBox };
            Log.Debug("Main Info Box: @{mainInfoBox}", mainInfoBox);
            Log.Debug("Extra Info Box: @{extraInfoBox}", extraInfoBox);

            var originalCard = await ParseOriginalCard(cardContext, context);
            var serialRarityPairs = serialMatcher.Matches(extraInfoBox["card set(s)"])
                .Where(m => (context.SetData != null) ? m.Groups[2].Value == context.SetData["prefix"] : true)
                .Select(m => (Serial: m.Groups[1].Value, Rarity: m.Groups[3].Value))
                .ToList();

            var originalSerialRarity = serialRarityPairs.First(p => nonFoilRarities.Contains(p.Rarity));
            serialRarityPairs.Remove(originalSerialRarity);
            originalCard.Serial = originalSerialRarity.Serial;
            originalCard.Rarity = originalSerialRarity.Rarity;

            if (originalCard.Set == null)
            {
                Log.Debug("Card was obtained from a Promo Set. trying to obtain the actual set from the serial.");
                if (context.Sets.TryGetValue(releaseIDMatcher.Match(originalCard.Serial).Groups[1].Value, out var set))
                    originalCard.Set = set;
//                else
//                    originalCard.Set = CreatePromoSet(cardContext);
                /*
                using (var db = _database()) {
                    originalCard.Set = await db.R4UReleaseSets.AsQueryable().Where(set => set.ReleaseCode == releaseIDMatcher.Match(originalCard.Serial).Groups[1].Value).FirstAsync();
                }
                */
            }
            yield return originalCard;

            foreach (var foilSRP in serialRarityPairs)
            {
                var foilCard = originalCard.Clone();
                foilCard.Rarity = foilSRP.Rarity;
                foilCard.Serial = foilSRP.Serial;
                foilCard.NonFoil = originalCard;
                yield return foilCard;
            }

            Log.Debug($"Original Card: {JsonSerializer.Serialize(originalCard)}");
            //await Task.Delay(60000);
            yield break;
        }

        private async Task<R4UCard> ParseOriginalCard(WikiCardContext context, WikiSetContext setContext)
        {
            var card = new R4UCard();
            var mainInfoBox = context.MainInfoBox;
            var extraInfoBox = context.ExtraInfoBox;
            card.Name = new MultiLanguageString();
            card.Name.JP = mainInfoBox.GetValueOrDefault("kanji", mainInfoBox["name"]);
            card.Name.EN = mainInfoBox["name"];
            card.Type = TranslateType(mainInfoBox["card type"]);
            card.Set = setContext.Set;
            if (card.Type == CardType.Character)
            {
                card.Cost = int.Parse(mainInfoBox["cost"]);
                card.ATK = int.Parse(mainInfoBox["atk"]);
                card.DEF = int.Parse(mainInfoBox["def"]);
                card.Traits = await ParseTraits(context.RawMainInfoBox["trait"], setContext);
            }
            if (extraInfoBox.TryGetValue("card flavor(s)", out var flavor))
            {
                card.Flavor = new MultiLanguageString();
                card.Flavor.EN = flavor;
            }
            if (extraInfoBox.TryGetValue("card abilities", out var abilities))
            {
                card.Effect = TranslateEffect(abilities);
                card.Color = CardUtils.InferFromEffect(card.Effect);
            }
            else
            {
                // Assumed. All Partner cards do not have effects, and vanilla cards have [Relaxing]
                card.Color = CardColor.Red;
            }

            return card;
        }

        private CardType TranslateType(string cardTypeString)
        {
            return cardTypeString.ToLower() switch
            {
                string s when s.StartsWith("character") => CardType.Character,
                string s when s.StartsWith("rebirth") => CardType.Rebirth,
                string s when s.StartsWith("partner") => CardType.Partner,
                _ => throw new NotImplementedException($"Cannot find CardType from {cardTypeString}")
            };
        }

        private MultiLanguageString[] TranslateEffect(string rawEffect)
        {
            return effectMatcher.Matches(rawEffect)
                .Select(m => new MultiLanguageString { EN = m.Groups[0].Value })
                .ToArray();
        }

        private async Task<List<MultiLanguageString>> ParseTraits(IHtmlTableCellElement traitCell, WikiSetContext setContext)
        {
            return await traitCell.ChildNodes
                .Where(x => x is IHtmlAnchorElement)
                .Cast<IHtmlAnchorElement>()
                .ToAsyncEnumerable()
                .SelectAwait(async a => await ParseTrait(a, setContext))
                .ToListAsync();
        }

        private async Task<MultiLanguageString> ParseTrait(IHtmlAnchorElement anchor, WikiSetContext context)
        {
            if (context.TraitData.TryGetValue(anchor.Href, out var mapResult)) return mapResult;
            var res = new MultiLanguageString();
            if (anchor.Relation != "nofollow")
            {
                Log.Information("Following link: {link}", anchor.Href);
                var document = await new Uri(anchor.Href).DownloadHTML(("Referer", anchor.BaseUri)).WithRetries(10);
                var traitData = CreateInfoBoxDataTable(document.QuerySelector(".portable-infobox"));
                res.EN = traitData["title"];
                res.JP = traitData["kanji"];
            }
            else
            {
                res.EN = anchor.TextContent;
            }
            context.TraitData.Add(anchor.Href, res);
            return res;
        }
    }

    internal class WikiSetContext
    {
        internal string URL { get; set; }
        internal Dictionary<string, string> SetData { get; set; }
        internal R4UReleaseSet Set { get; set; }
        internal Dictionary<string, R4UReleaseSet> Sets { get; set; } = new Dictionary<string, R4UReleaseSet>(); 
        internal Dictionary<string, MultiLanguageString> TraitData { get; set; } = new Dictionary<string, MultiLanguageString>();
    }

    internal class WikiCardContext
    {
        internal Dictionary<string, IHtmlTableCellElement> RawMainInfoBox { get; set; }
        internal Dictionary<string, string> MainInfoBox { get; set; }
        internal Dictionary<string, string> ExtraInfoBox { get; set; }
    }
}
