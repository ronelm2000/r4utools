using Fluent.IO;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Exporters
{
    /// <summary>
    /// A Deck Exporter whose output is purely a JSON file that coincides with the format of WeissSchwarzDeck, exactly.
    /// </summary>
    public class LocalDeckJSONExporter : IDeckExporter
    {
        private ILogger Log = Serilog.Log.ForContext<LocalDeckJSONExporter>();
        private JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public string[] Alias => new[]{ "local", "json" };

        public async Task Export(R4UDeck deck, IExportInfo info)
        {
            Log.Information("Exporting as Deck JSON.");
            var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"deck_{deck.Name.AsFileNameFriendly()}.json");
            await Export(deck, info, jsonFilename);
        }

        public async Task Export(R4UDeck deck, IExportInfo info, Path jsonFilename)
        {
            var simplifiedDeck = new
            {
                Name = deck.Name,
                Remarks = deck.Remarks,
                Ratios = deck.AsSimpleDictionary()
            };

            await using (var stream = jsonFilename.OpenStream(System.IO.FileMode.Create))
                await JsonSerializer.SerializeAsync(stream, simplifiedDeck, options: _defaultOptions);

            Log.Information($"Done: {jsonFilename.FullPath}");

            if (!String.IsNullOrWhiteSpace(info?.OutCommand))
                await ExecuteCommandAsync(info.OutCommand, jsonFilename);
        }

        private async Task ExecuteCommandAsync(string outCommand, Path jsonFilename)
        {
            ConsoleUtils.RunExecutable(outCommand, $"\"{jsonFilename.FullPath}\"");
            await Task.CompletedTask;
        }
    }
}
