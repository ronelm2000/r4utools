using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.CLI
{
    [Verb("parse", HelpText = "Exports a card release set into the local database, so that it may be used to export decks later.")]
    public class ParseVerb : IVerbCommand, IParseInfo
    {
        [Value(0, HelpText = "URL to parse. Compatible Formats are found at https://github.com/ronelm2000/wsmtools/")]
        public string URI { get; set; }

        [Option("with", HelpText = "Provides a hint as to what parser should be used.", Default = new string[] { })]
        public IEnumerable<string> ParserHints { get; set; }

        public async Task Run(IContainer container)
        {
            var Log = Serilog.Log.ForContext<ParseVerb>();

            var parser = container.GetAllInstances<ICardSetParser>()
                .Where(parser => parser.IsCompatible(this))
                .First();

            var cardList = await parser.Parse(URI).ToListAsync();
            var cards = cardList.Distinct(R4UCard.SerialComparer).ToAsyncEnumerable();

            var postProcessors = container.GetAllInstances<ICardPostProcessor>()
                .ToAsyncEnumerable()
                .WhereAwait(async processor => await processor.IsCompatible(cardList))
                .Where(processor => (parser is IFilter<ICardPostProcessor> filter) ? filter.IsIncluded(processor) : true)
                .WhereAwait(async processor => (processor is ISkippable<IParseInfo> skippable) ? await skippable.IsIncluded(this) : true)
                .WhereAwait(async processor => (processor is ISkippable<ICardSetParser> skippable) ? await skippable.IsIncluded(parser) : true)
                .OrderByDescending(processor => processor.Priority);

            cards = await postProcessors.AggregateAsync(cards, (pp, cs) => cs.Process(pp));

            using (var db = container.GetInstance<CardDatabaseContext>()) await db.Database.MigrateAsync();
            var finalList = await cards.ToListAsync();
            var finalListSerials = finalList.Select(c => c.Serial);

            using (var db = container.GetInstance<CardDatabaseContext>())
            {
                var dupQuery = db.R4UCards.AsQueryable<R4UCard>().Where(c => finalListSerials.Contains(c.Serial)).AsEnumerable();
                db.R4UCards.RemoveRange(dupQuery);
                await db.AddRangeAsync(finalList);
                Log.Information("Added to DB: {serials:lj}", finalListSerials);
                await db.SaveChangesAsync();
            }

            Log.Information("Successfully parsed: {uri}", URI);
        }
    }
}
