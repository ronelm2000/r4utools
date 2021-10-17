using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Lamar;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Montage.RebirthForYou.Tools.CLI.Utilities.RebirthForYou;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class CSVPartnerOnlyParser : ICardSetParser
    {
        ILogger _log = Serilog.Log.ForContext<CSVPartnerOnlyParser>();
        private Func<CardDatabaseContext> _database;

        public CSVPartnerOnlyParser(IContainer ioc)
        {
            //Log ??= Serilog.Log.ForContext<FandomWikiSetParser>();
            _database = () => ioc.GetService<CardDatabaseContext>();
        }

        public bool IsCompatible(IParseInfo parseInfo)
        {
            try
            {
                var path = Fluent.IO.Path.Get(parseInfo.URI);
                if (!path.Exists)
                {
                    _log.Debug("The provided path [{path}] does not exist. Will reject.", path.FullPath);
                    return false;
                }
                else if (path.Extension.ToLower() != ".csv")
                {
                    _log.Debug("The provided path [{path}] is not CSV. Will reject.", path.FullPath);
                    return false;
                }
                else if (!parseInfo.ParserHints.Contains("partnersonly"))
                {
                    _log.Information("Cannot use because its intention is to only parse partners; if your intention is to use this parser, please put [--with partnersonly].");
                    return false;
                }
                else
                {
                    return true;
                }                
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            var setCache = new Dictionary<string, R4UReleaseSet>();

            using (var reader = new System.IO.StreamReader(urlOrLocalFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await foreach (var entry in csv.GetRecordsAsync<SerialRecord>())
                {
                    if (string.IsNullOrWhiteSpace(entry.Serial)
                        || string.IsNullOrWhiteSpace(entry.CardType)
                        || string.IsNullOrWhiteSpace(entry.Name)
                        ) continue;

                    var card = new R4UCard
                    {
                        Serial = entry.Serial,
                        Name = new MultiLanguageString
                        {
                            EN = entry.Name
                        },
                        Type = CardType.Partner
                    };
                    var releaseID = R4URegex.ReleaseIDMatcher.Match(card.Serial).Groups[1].Value;
                    if (!setCache.ContainsKey(releaseID))
                    {
                        setCache[releaseID] = FindFromDB(releaseID) ?? CreateTemporarySet(releaseID);
                    }
                    card.Set = setCache[releaseID];
                    yield return card;
                }
            }
        }
        private R4UReleaseSet FindFromDB(string releaseID)
        {
            using (var db = _database())
            {
                return db.R4UReleaseSets
                    .AsQueryable()
                    .Where(s => s.ReleaseCode == releaseID)
                    .FirstOrDefault();
            }
        }

        private R4UReleaseSet CreateTemporarySet(string releaseCode)
        {
            return new R4UReleaseSet()
            {
                ReleaseCode = releaseCode
            };
        }


    }

    internal class SerialRecord
    {
        [Name("serial")]
        public string Serial { get; set; }
        [Name("name")]
        public string Name { get; set; }
        [Name("type")]
        public string CardType { get; set; }
    }

}
