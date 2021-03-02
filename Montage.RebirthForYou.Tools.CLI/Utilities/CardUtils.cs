using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class CardUtils
    {
        public static CardColor InferFromEffect(MultiLanguageString[] effect)
        {
            return effect?.Append(null).First() switch
            {
                MultiLanguageString mls when mls.EN.StartsWith("[Spark]") => CardColor.Yellow,
                MultiLanguageString mls when mls.EN.StartsWith("[Blocker") => CardColor.Green,
                MultiLanguageString mls when mls.EN.StartsWith("[Cancel") => CardColor.Green,
                _ => CardColor.Blue
            };
        }
    }
}
