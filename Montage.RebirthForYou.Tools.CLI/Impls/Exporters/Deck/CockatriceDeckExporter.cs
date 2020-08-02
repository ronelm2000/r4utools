using Fluent.IO;
using Lamar;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.External.Cockatrice;
using Montage.RebirthForYou.Tools.CLI.Impls.Inspectors.Deck;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Exporters.Deck
{
    public class CockatriceDeckExporter : IDeckExporter, IFilter<IExportedDeckInspector>
    {
        private readonly ILogger Log;
        private Func<CardDatabaseContext> _database;
        private XmlSerializer _serializer = new XmlSerializer(typeof(CockatriceDeck));

        public string[] Alias => new[] { "cockatrice", "cckt3s" };

        public CockatriceDeckExporter(IContainer container)
        {
            Log = Serilog.Log.ForContext<CockatriceDeckExporter>();
            _database = () => container.GetInstance<CardDatabaseContext>();
            /*
            _parse = async (url) =>
            {
                var parser = container.GetInstance<ParseVerb>();
                parser.URI = url;
                await parser.Run(container);
            };
            */
        }
        public async Task Export(R4UDeck deck, IExportInfo info)
        {
            Log.Information("Serializing: {name}", deck.Name);

            /*
            using (var db = _database())
            {
                Log.Information("Replacing all foils with non-foils...");
                foreach (var card in deck.Ratios.Keys)
                    if (card.IsFoil) deck.ReplaceCard(card, await db.FindNonFoil(card));
            }
            */

            Log.Information("Creating deck.cod...");
            var cckDeck = new CockatriceDeck();
            cckDeck.DeckName = deck.Name;
            cckDeck.Comments = deck.Remarks;
            cckDeck.Ratios = new CockatriceDeckRatio();
            cckDeck.Ratios.Ratios = deck.Ratios.Select(Translate).ToList();

            var resultDeck = Path.CreateDirectory(info.Destination).Combine($"{deck.Name?.AsFileNameFriendly() ?? "deck"}.cod");
            resultDeck.Open(s => _serializer.Serialize(s, cckDeck),
                                    System.IO.FileMode.Create,
                                    System.IO.FileAccess.Write,
                                    System.IO.FileShare.ReadWrite
                                    );
            Log.Information($"Saved: {resultDeck.FullPath}");
            await Task.CompletedTask;
        }

        private Type[] _exclusionFilters = new[]
        {
            typeof(CachedImageInspector),
            typeof(SanityImageInspector), 
            typeof(SanityTranslationsInspector)
        };
        public bool IsIncluded(IExportedDeckInspector item)
        {
            return item.GetType() switch
            {
                var t when _exclusionFilters.Contains(t) => false,
                _ => true
            };
        }

        private CockatriceSerialAmountPair Translate(KeyValuePair<R4UCard, int> cardAmountPair)
        {
            return new CockatriceSerialAmountPair()
            {
                Serial = cardAmountPair.Key.Serial,
                Amount = cardAmountPair.Value
            };
        }


    }
}
