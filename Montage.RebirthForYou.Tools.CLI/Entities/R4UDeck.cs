using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class R4UDeck
    {
        public string Name { get; set; }
        public Dictionary<R4UCard, int> Ratios { get; set; } = new Dictionary<R4UCard, int>();
        public string Remarks { get; set; }

        public int Count => Ratios.Values.Sum();
        internal Dictionary<string,int> AsSimpleDictionary()
        {
            return Ratios.Select(kyd => (kyd.Key.Serial, kyd.Value))
                         .ToDictionary(kyd => kyd.Serial, kyd => kyd.Value);
        }

        public R4UDeck Clone()
        {
            var res = new R4UDeck();
            res.Ratios = this.Ratios.ToDictionary(kyd => kyd.Key, kyd => kyd.Value);
            res.Name = this.Name;
            res.Remarks = this.Remarks;
            return res;
        }

        /// <summary>
        /// Replaces a card with a new card, using the same ratio as the old card.
        /// Returns false if the old card cannot be found on the deck.
        /// </summary>
        /// <param name="oldCard"></param>
        /// <param name="newCard"></param>
        public bool ReplaceCard(R4UCard oldCard, R4UCard newCard)
        {
            if (newCard == null)
            {
                return false;
            }
            else if (Ratios.TryGetValue(oldCard, out int oldRatio))
            {
                Ratios[newCard] = oldRatio;
                Ratios.Remove(oldCard);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static readonly R4UDeck Empty = new R4UDeck();
    }
}
