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
        private readonly XmlSerializer _serializer = new(typeof(CockatriceDeck));

        public string[] Alias => new[] { "cockatrice", "cckt3s" };

        public CockatriceDeckExporter(IContainer container)
        {
            Log = Serilog.Log.ForContext<CockatriceDeckExporter>();
        }
        public async Task Export(R4UDeck deck, IExportInfo info)
        {
            Log.Information("Serializing: {name}", deck.Name);
            Log.Information("Creating deck.cod...");
            var cckDeck = new CockatriceDeck
            {
                DeckName = deck.Name,
                Comments = deck.Remarks,
                Ratios = new CockatriceDeckRatio()
                {
                    Ratios = deck.Ratios.Select(Translate).ToList()
                }
            };

            var resultDeck = Path.CreateDirectory(info.Destination).Combine($"{deck.Name?.AsFileNameFriendly() ?? "deck"}.cod");
            resultDeck.Open(s => _serializer.Serialize(s, cckDeck),
                                    System.IO.FileMode.Create,
                                    System.IO.FileAccess.Write,
                                    System.IO.FileShare.ReadWrite
                                    );
            Log.Information($"Saved: {resultDeck.FullPath}");
            await Task.CompletedTask;
        }

        private readonly Type[] _exclusionFilters = new[]
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
