using Lamar;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Migrations;
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
            var db = _database();
            var dbSets = await db.R4UReleaseSets.ToAsyncEnumerable().Where(s => sets.ContainsKey(s.ReleaseCode)).ToListAsync();
            db.R4UReleaseSets.RemoveRange(dbSets);

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
