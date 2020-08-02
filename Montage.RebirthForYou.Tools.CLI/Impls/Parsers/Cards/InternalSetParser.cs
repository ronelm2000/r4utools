using AngleSharp.Common;
using AngleSharp.Media.Dom;
using Fluent.IO;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class InternalSetParser : ICardSetParser
    {
        private ILogger Log = Serilog.Log.ForContext<InternalSetParser>();
        //private JsonSerializer _jsonSerializer = new JsonSerializer();

        public bool IsCompatible(IParseInfo info)
        {
            return Fluent.IO.Path.Get(info.URI).Extension == ".r4uset";
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            Log.Information("Starting...");
            using (Stream s = Fluent.IO.Path.Get(urlOrLocalFile).GetStream())
            {
                var jsonObject = await JsonSerializer.DeserializeAsync<InternalCardSet>(s);
                var sets = jsonObject.Cards
                    .Select(c => c.Set)
                    .Distinct(R4UReleaseSet.ByReleaseCode)
                    .ToDictionary(set => set.ReleaseCode);
                var cards = jsonObject.Cards;
                foreach (var card in cards)
                {
                    if (sets.ContainsKey(card.Set.ReleaseCode))
                        card.Set = sets[card.Set.ReleaseCode];
                    yield return card;

                    foreach (var alt in card.Alternates)
                    {
                        alt.NonFoil = card;
                        alt.FillProxy();
                        yield return alt;
                    }
                }
            }
        }
    }

    public class InternalCardSet
    {
        public int Version { get; set; }
        public List<R4UCard> Cards { get; set; }
    }
}
