using Fluent.IO;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.Exceptions;
using Montage.RebirthForYou.Tools.CLI.Entities.JSON;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
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
            await using var stream = await Path.Get(sourceUrlOrFile).OpenStreamAsync(System.IO.FileMode.Open);
            return await Parse(stream, CancellationToken.None);
        }

        public async Task<R4UDeck> Parse(System.IO.Stream sourceStream, CancellationToken token = default) { 
            SimpleDeck deckJSON = null;
            deckJSON = await JsonSerializer.DeserializeAsync<SimpleDeck>(sourceStream, cancellationToken: token);
            R4UDeck deck = new R4UDeck
            {
                Name = deckJSON.Name,
                Remarks = deckJSON.Remarks
            }; 
            using var db = _database();
            await db.Database.MigrateAsync(token);
            foreach (var serial in deckJSON.Ratios.Keys)
            {
                var card = await db.R4UCards.FindAsync(new[] { serial }, cancellationToken: token);
                if (card == null)
                {
                    Log.Error("This card is missing in your local card db: {serial}", serial);
                    Log.Error("You must obtain information about this card first using the command {cmd}", "./wstools parse");
                    throw new DeckParsingException($"This card is missing in your local card db: {serial}");
                }
                else
                {
                    deck.Ratios[card] = deckJSON.Ratios[serial];
                }
            }
            return deck;
        }
    }
}
