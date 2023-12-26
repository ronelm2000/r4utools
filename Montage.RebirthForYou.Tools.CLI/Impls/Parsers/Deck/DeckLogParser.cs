using AngleSharp.Dom;
using Flurl.Http;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.Exceptions;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Newtonsoft.Json;
using Octokit;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck;

/// <summary>
/// Implements a Deck Parser that sources deck information from DeckLog.
/// Note that parsing the deck this way means the deck has no name or description, but the source link will be appended.
/// </summary>
public class DeckLogParser : IDeckParser
{
    private ILogger Log = Serilog.Log.ForContext<DeckLogParser>();
    private readonly Func<CardDatabaseContext> _database;

    private readonly DeckLogAPIProfile[] _deckLogProfiles = new[]
    {
        new DeckLogAPIProfile {
            URLMatcher = new Regex(@"(.*):\/\/decklog\.bushiroad\.com\/view\/([^\?]*)(.*)"),
            ViewAPI = "https://decklog.bushiroad.com/system/app/api/view"
        },
        new DeckLogAPIProfile {
            URLMatcher = new Regex(@"(.*):\/\/decklog-en\.bushiroad\.com\/ja\/view\/([^\?]*)(.*)"),
            ViewAPI = "https://decklog-en.bushiroad.com/system/app-ja/api/view"
        }
    };

    public string[] Alias => new[] { "decklog" };

    public int Priority => 1;

    public DeckLogParser(IContainer ioc)
    {
        this._database = () => ioc.GetInstance<CardDatabaseContext>();
    }

    public bool IsCompatible(string urlOrFile)
    {
        if (!Uri.TryCreate(urlOrFile, UriKind.Absolute, out var _))
            return false;
        else if (_deckLogProfiles.Any(profile => profile.URLMatcher.IsMatch(urlOrFile)))
            return true;
        else
            return false;
    }

    public async Task<R4UDeck> Parse(string sourceUrlOrFile)
    {
        var profile = _deckLogProfiles.First(profile => profile.URLMatcher.IsMatch(sourceUrlOrFile));
        var document = await sourceUrlOrFile.WithHTMLHeaders().GetHTMLAsync();
        var deckID = profile.URLMatcher.Match(sourceUrlOrFile).Groups[2];
        Log.Information("Parsing ID: {deckID}", deckID);
        var response = await $"{profile.ViewAPI}/{deckID}" //
            .WithReferrer(sourceUrlOrFile) //
            .PostJsonAsync(null);
        var json = JsonConvert.DeserializeObject<dynamic>(await response.ResponseMessage.Content.ReadAsStringAsync());
        //var json = JsonConverter.CreateDefault().Deserialize<dynamic>(new JsonReader(await response.Content.ReadAsStreamAsync()));
        var newDeck = new R4UDeck();
        var missingSerials = new List<string>();
        newDeck.Name = json.title.ToString();
        newDeck.Remarks = json.memo.ToString();
        using (var db = _database())
        {
            List<dynamic> items = new List<dynamic>();
            items.AddRange(json.list);
            items.AddRange(json.sub_list);
            foreach (var cardJSON in items)
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
                    Log.Debug("Adding: {card} [{quantity}]", card?.Serial, quantity);
                    if (newDeck.Ratios.TryGetValue(card, out int oldVal))
                        newDeck.Ratios[card] = oldVal + quantity;
                    else
                        newDeck.Ratios.Add(card, quantity);
                } else
                {
                    missingSerials.Add(serial);
                    //throw new DeckParsingException($"MISSING_SERIAL_{serial}");
                    Log.Warning("Serial has been effectively skipped because it's not found on the local db: [{serial}]", serial);
                }
            }
        }
        if (missingSerials.Count > 0)
            throw new DeckParsingException($"The following serials are missing from the DB:\n{missingSerials.ConcatAsString("\n")}");
        else
        {
            Log.Debug($"Result Deck: {JsonConvert.SerializeObject(newDeck.AsSimpleDictionary())}");
            return newDeck;
        }
    }
}

public record DeckLogAPIProfile
{
    public Regex URLMatcher { get; init; }
    public string ViewAPI { get; init; }
}
