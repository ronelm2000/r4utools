using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Newtonsoft.Json;
using Octokit;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck
{
    /// <summary>
    /// Implements a Deck Parser that sources deck information from DeckLog.
    /// Note that parsing the deck this way means the deck has no name or description, but the source link will be appended.
    /// </summary>
    public class DeckLogParser : IDeckParser
    {
        private Regex urlMatcher = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)");
        private string deckLogApiUrlPrefix = "https://decklog.bushiroad.com/system/app/api/view/";
        private ILogger Log = Serilog.Log.ForContext<DeckLogParser>();
        private readonly Func<CardDatabaseContext> _database;

        public string[] Alias => new[] { "decklog" };

        public int Priority => 1;

        public DeckLogParser(IContainer ioc)
        {
            this._database = () => ioc.GetInstance<CardDatabaseContext>();
        }

        public bool IsCompatible(string urlOrFile)
        {
            if (Uri.TryCreate(urlOrFile, UriKind.Absolute, out _))
            {
                return urlMatcher.IsMatch(urlOrFile);
            }else
            {
                return false;
            }
        }

        public async Task<R4UDeck> Parse(string sourceUrlOrFile)
        {
            var document = await sourceUrlOrFile.WithHTMLHeaders().GetHTMLAsync();
            //var deckView = document.QuerySelector(".deckview");
            //var cardControllers = deckView.QuerySelectorAll(".card-controller-inner");
            var deckID = urlMatcher.Match(sourceUrlOrFile).Groups[2];
            Log.Information("Parsing ID: {deckID}", deckID);
            var response = await $"{deckLogApiUrlPrefix}{deckID}" //
                .WithReferrer(sourceUrlOrFile) //
                .PostJsonAsync(null);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            //var json = JsonConverter.CreateDefault().Deserialize<dynamic>(new JsonReader(await response.Content.ReadAsStreamAsync()));
            var newDeck = new R4UDeck();
            newDeck.Name = json.title.ToString();
            newDeck.Remarks = json.memo.ToString();
            using (var db = _database())
            {
                foreach (var cardJSON in json.list)
                {
                    string serial = cardJSON.card_number.ToString();
                    serial = serial.Replace('＋', '+');
                    if (serial == null)
                    {
                        Log.Warning("serial is null for some reason!");
                    }
                    var card = await db.R4UCards.FindAsync(serial);
                    int quantity = cardJSON.num;
                    if (card != null)
                    {
                        newDeck.Ratios.Add(card, quantity);
                    } else
                    {
                        Log.Warning("Serial has been effectively skipped because it's not found on the local db: [{serial}]", serial);
                    }
                }
            }
            Log.Debug($"Result Deck: {JsonConvert.SerializeObject(newDeck.AsSimpleDictionary())}");
            return newDeck;
        }
    }
}
