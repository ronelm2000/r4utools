using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.PostProcessors
{
    public class CardStringNormalizer : ICardPostProcessor, ISkippable<ICardSetParser>
    {
        public int Priority => 3;

        public ILogger Log = Serilog.Log.Logger.ForContext<CardStringNormalizer>();

        public Task<bool> IsCompatible(List<R4UCard> cards) => Task.FromResult(true);

        public async Task<bool> IsIncluded(ICardSetParser parser)
        {
            return await new ValueTask<bool>(!(parser is InternalSetParser));
        }

        public async IAsyncEnumerable<R4UCard> Process(IAsyncEnumerable<R4UCard> originalCards)
       {
            await foreach (var card in originalCards)
            {
                card.Name.EN = Normalize(card.Name.EN);
                foreach (var trait in card.Traits ?? new List<MultiLanguageString>())
                    trait.EN = Normalize(trait.EN);
                foreach (var eff in card.Effect ?? Array.Empty<MultiLanguageString>())
                    eff.EN = Normalize(eff.EN);
                yield return card;
            }
        }

        private string Normalize(string original) => original?.ReplaceAll(
                    ("’", "'"),
                    ("“", "\""),
                    ("”", "\"")
                    ) ?? null;
    }
}
