using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Impls.PostProcessors
{
    public class AlternatePostProcessor : ICardPostProcessor
    {
        public int Priority => 2;

        public bool IsCompatible(List<R4UCard> cards)
        {
            return true;
        }

        public async IAsyncEnumerable<R4UCard> Process(IAsyncEnumerable<R4UCard> originalCards)
        {
            var result = await originalCards.Distinct(s => s.Serial).ToDictionaryAsync(c => c.Serial);
            foreach (var card in result.Values)
            {
                if (card.NonFoil != null)
                    card.NonFoil = result[card.NonFoil.Serial];
                yield return card;
            }
        }
    }
}
