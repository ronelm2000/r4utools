using Fluent.IO;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Entities.JSON;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Exporters.Database
{
    public class LocalDatabaseExporter : IDatabaseExporter
    {
        private ILogger Log = Serilog.Log.ForContext<LocalDatabaseExporter>();
        private JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public string[] Alias => new string[] { "local", "r4uset" };

        public async Task Export(CardDatabaseContext database, IDatabaseExportInfo info)
        {
            Log.Information("Starting...");
            var sets = await database.R4UReleaseSets
                .Include(s => s.Cards)
                .ThenInclude(c => c.Name)
                .Include(s => s.Cards)
                .ThenInclude(c => c.Traits)
                .Include(s => s.Cards)
                .Where(set => info.ReleaseIDs.Contains(set.ReleaseCode))
                .ToListAsync();
            //var cards2 = await database.R4UCards.Where(c => sets.Contains(c.Set)).ToList();
            var cards = sets
                .SelectMany(s => s.Cards ?? new R4UCard[] { })
                .Where(c => !c.IsFoil)
                .Select(c => PostProcess(c))
                .ToList();
            var newCardSet = new InternalCardSet { Version = 1, Cards = cards };

            var jsonFilename = Path.CreateDirectory(info.Destination).Combine($"set_{info.ReleaseIDs.ConcatAsString("_").AsFileNameFriendly()}.r4uset");

            var attempts = 1;
            do try
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(newCardSet, options: _defaultOptions);
                    jsonFilename.Write(bytes);
                    /*
                    jsonFilename.Open(
                        s => Serialize(s, newCardSet, options: _defaultOptions),
                        System.IO.FileMode.Create,
                        System.IO.FileAccess.Write,
                        System.IO.FileShare.ReadWrite
                    );
                    */
                    break;
                }
                catch (Exception)
                {
                    if (attempts++ > 4) throw;
                } while (true);

            Log.Information("Completed: {jsonFilename:l}", jsonFilename.FullPath);
        }

        private R4UCard PostProcess(R4UCard card)
        {
            var newCard = card.Clone();
            newCard.Alternates = card.Alternates?.Select(c => c.AsProxy()).ToList();
            return newCard;
        }

        private IAsyncEnumerable<R4UCard> CreateQuery(IAsyncEnumerable<R4UCard> query, IDatabaseExportInfo info)
        {
            var releaseIDLimitations = info.ReleaseIDs.Select(s => s.ToLower()).ToArray();
            var result = query;
            if (releaseIDLimitations.Length > 0)
                result = result.Where(card => releaseIDLimitations.Contains(card.Set?.ReleaseCode.ToLower()));
            return result; //.Where(card => card.Images.Count > 0);
        }
    }
}

