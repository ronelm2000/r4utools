using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Migrations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.PostProcessors
{
    public class ReferenceFixPostProcessor : ICardPostProcessor
    {
        private Func<CardDatabaseContext> _database;
        private ILogger Log = Serilog.Log.ForContext<ReferenceFixPostProcessor>();

        public int Priority => 0;

        public ReferenceFixPostProcessor(IContainer ioc)
        {
            _database = () => ioc.GetService<CardDatabaseContext>();
        }

        public Task<bool> IsCompatible(List<R4UCard> cards) => Task.FromResult(true);

        public async IAsyncEnumerable<R4UCard> Process(IAsyncEnumerable<R4UCard> originalCards)
        {
            var result = await originalCards.Distinct(s => s.Serial).ToDictionaryAsync(c => c.Serial);
            var sets = await result.Values.Select(c => c.Set).ToAsyncEnumerable().Distinct(s => s.ReleaseCode).ToDictionaryAsync(s => s.ReleaseCode);
            using (var db = _database())
            {
                var dbSets = await db.R4UReleaseSets
                    .AsQueryable()
                    .Include(s => s.Cards)
                    .ThenInclude(c => c.Traits)
                    .ToAsyncEnumerable()
                    .Where(s => sets.ContainsKey(s.ReleaseCode))
                    .ToListAsync();
                var cardsInSets = dbSets.SelectMany(s => s.Cards ?? new R4UCard[] { }).ToList();
                var cardsToMerge = cardsInSets
                    .Where(c => !result.ContainsKey(c.Serial))
                    .ToList();
                Log.Information("Attempting to merge with {count} cards from DB.", cardsToMerge.Count);

                foreach (var card in cardsToMerge)
                    result[card.Serial] = card.Clone();
                
                if (dbSets.Count > 0)
                    db.R4UReleaseSets.RemoveRange(dbSets);

                var traits = dbSets
                    .SelectMany(s => s.Cards)
                    .SelectMany(c => c.Traits);
                
                db.RemoveRange(traits);
                
                await db.SaveChangesAsync();
            }

            foreach (var card in result.Values)
            {
                if (card.Alternates?.Count > 0)
                {
                    card.Alternates = result.Select(x => x.Value).Where(c => c.NonFoil?.Serial == card.Serial).ToList();
                }
                if (card.NonFoil != null)
                {
                    card.NonFoil = result[card.NonFoil.Serial];
                }
                if (card.Set != null)
                    card.Set = sets[card.Set.ReleaseCode];
                yield return card;
            }
        }
    }
}
