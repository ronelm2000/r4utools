using Avalonia.X11;
using Microsoft.EntityFrameworkCore.Internal;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Montage.RebirthForYou.Tools.GUI.ModelViews
{
    public struct CardQuery
    {
        public string Serial;
        public string Name;
        public string NeoStandardCode; //TODO: Support NS Code compatibility.
        public string Effect;
        public int? Cost;
        public int? ATK;
        public int? DEF;
        public string[] Traits;

        // Compound Queries:
        [JsonProperty("or")]
        public CardQuery[] Or;
        [JsonProperty("and")]
        public CardQuery[] And;

        public Predicate<R4UCard> ToQuery()
        {
            if (Or?.Length > 1)
                return Or.Select(q => q.ToQuery()).Aggregate(OrAggregate());
            else if (And?.Length > 1)
                return And.Select(q => q.ToQuery()).Aggregate(AndAggregate());

            List<Predicate<R4UCard>> results = new List<Predicate<R4UCard>>();
            CardQuery _this = this;
            if (Serial != null)
                results.Add((card) => card.Serial.Contains(_this.Serial));
            if (Name != null)
                results.Add((card) => (card.Name.EN ?? "").Contains(_this.Name) || (card.Name.JP ?? "").Contains(_this.Name));
            if (Effect != null)
                results.Add((card) => card.Effect?.Any(eff => (eff.EN ?? "").Contains(_this.Effect) || (eff.JP ?? "").Contains(_this.Effect)) ?? false);
            if (Cost != null)
                results.Add((card) => card.Cost == _this.Cost);
            if (ATK != null)
                results.Add((card) => card.ATK == _this.ATK);
            if (DEF != null)
                results.Add((card) => card.DEF == _this.DEF);
            if (Traits != null)
                results.Add((card) => card.Traits?.Any(x => _this.Traits.Any(t => x.EN.Contains(t) || x.JP.Contains(t))) ?? false);
            if (results.Count > 0)
                return results.Aggregate(AndAggregate());
            else
                return (card) => true;
        }

        private static Func<Predicate<R4UCard>, Predicate<R4UCard>, Predicate<R4UCard>> AndAggregate()
        {
            return (p1, p2) => (card) => p1(card) && p2(card);
        }

        private static Func<Predicate<R4UCard>, Predicate<R4UCard>, Predicate<R4UCard>> OrAggregate()
        {
            return (p1, p2) => (card) => p1(card) || p2(card);
        }

        public static bool TryParse(string jsonString, out CardQuery? query)
        {
            try
            {
                query = JsonConvert.DeserializeObject<CardQuery>(jsonString);
                return true;
            } catch
            {
                query = null;
                return false;
            }
        }
    }
}
