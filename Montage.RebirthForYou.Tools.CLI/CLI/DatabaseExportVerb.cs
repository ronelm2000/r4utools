﻿using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.CLI
{
    [Verb("export-db", HelpText = "Exports the card database using a specified exporter algorithm.")]
    public class DatabaseExportVerb : IVerbCommand, IDatabaseExportInfo
    {
        [Value(0, HelpText = "Indicates the destination; usually a folder.", Default = "./Export/")]
        public string Destination { get; set; } = "./Export/";

        [Option("rids", HelpText = "Limits the range of the database export to a few RIDs (Release IDs).", Separator = ',', Default = new string[] { })]
        public IEnumerable<string> ReleaseIDs { get; set; } = new string[] { };

        [Value(0, HelpText = "Indicates the source file/url. Default value: ./cards.db", Default = "./cards.db")]
        public string Source { get; set; } = "./cards.db";

        public string Parser => null;

        [Option("exporter", HelpText = "Manually sets the database exporter to use. Possible values: cockatrice", Default = "local")]
        public string Exporter { get; set; } = "local";

        [Option("out", HelpText = "For some exporters, gives an out command to execute after exporting.", Default = "")]
        public string OutCommand { get; set; } = "";

        [Option("with", HelpText = "For some exporters, enables various flags. See each exporter for details.", Separator = ',', Default = new string[] { })]
        public IEnumerable<string> Flags { get; set; } = new string[] { };

        [Option("noninteractive", HelpText = "When set to true, there will be no prompts. Default options will be used.", Default = false)]
        public bool NonInteractive { get; set; } = false;

        [Option("nowarn", HelpText = "When set to true, all warning prompts will default to yes without user input. This flag when set ignores noninteractive flag during warnings (and is automatically true).", Default = false)]
        public bool NoWarning { get; set; } = false;

        private readonly ILogger Log = Serilog.Log.ForContext<DatabaseExportVerb>();

        private static readonly IEnumerable<string> Empty = new string[] { };

        /// <summary>
        /// For the IOC
        /// </summary>
        public DatabaseExportVerb()
        { 
        }

        public async Task Run(IContainer ioc)
        {
            if (NoWarning) NonInteractive = true;

            Log.Information("Running...");

            using (var database = ioc.GetInstance<CardDatabaseContext>())
            {
                await database.Database.MigrateAsync();
                var exporter = ioc.GetAllInstances<IDatabaseExporter>()
                    .Where(exporter => exporter.Alias.Contains(Exporter))
                    .First();

                await exporter.Export(database, this);
            }
            /*

            var deck = await parser.Parse(Source);
            var inspectionOptions = new InspectionOptions()
            {
                IsNonInteractive = this.NonInteractive,
                NoWarning = this.NoWarning
            };
            deck = await ioc.GetAllInstances<IExportedDeckInspector>()
                .OrderByDescending(inspector => inspector.Priority)
                .ToAsyncEnumerable()
                .AggregateAwaitAsync(deck, async (d, inspector) => await inspector.Inspect(d, inspectionOptions));

            if (deck != WeissSchwarzDeck.Empty)
            {
                var exporter = ioc.GetAllInstances<IDeckExporter>()
                    .Where(exporter => exporter.Alias.Contains(Exporter))
                    .First();

                await exporter.Export(deck, this);
            }
            */
        }
    }
}
