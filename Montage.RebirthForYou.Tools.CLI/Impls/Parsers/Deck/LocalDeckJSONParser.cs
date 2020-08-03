using Fluent.IO;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.Exceptions;
using Montage.RebirthForYou.Tools.CLI.Entities.JSON;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck
{
    public class LocalDeckJSONParser : IDeckParser
    {
        private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONParser>();
        public string[] Alias => new[] { "local", "json" };
        public int Priority => 1;
        private readonly Func<CardDatabaseContext> _database;

        public LocalDeckJSONParser(IContainer container)
        {
            _database = () => container.GetInstance<CardDatabaseContext>();
        }


        public bool IsCompatible(string urlOrFile)
        {
            var filePath = Path.Get(urlOrFile);
            if (!filePath.Exists)
                return false;
            else if (filePath.Extension != ".json")
                return false;
            else if (filePath.Extension != ".r4udek")
                return false;
            else
                return true;
        }

        public async Task<R4UDeck> Parse(string sourceUrlOrFile)
        {
            var filePath = Path.Get(sourceUrlOrFile);
            SimpleDeck deckJSON = null;
            deckJSON = JsonSerializer.Deserialize<SimpleDeck>(filePath.ReadBytes());
            R4UDeck deck = new R4UDeck();
            deck.Name = deckJSON.Name;
            deck.Remarks = deckJSON.Remarks;
            using (var db = _database())
            {
                await db.Database.MigrateAsync();
                foreach (var serial in deckJSON.Ratios.Keys)
                {
                    var card = await db.R4UCards.FindAsync(serial);
                    if (card == null)
                    {
                        Log.Error("This card is missing in your local card db: {serial}", serial);
                        Log.Error("You must obtain information about this card first using the command {cmd}", "./wstools parse");
                        throw new DeckParsingException($"This card is missing in your local card db: {serial}");
                        return R4UDeck.Empty;
                    }
                    else
                    {
                        deck.Ratios[card] = deckJSON.Ratios[serial];
                    }
                }
            }
            return deck;
        }
    }
}
