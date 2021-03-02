using AngleSharp.Common;
using AngleSharp.Media.Dom;
using Fluent.IO;
using Flurl.Http;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Cards
{
    public class InternalSetParser : ICardSetParser
    {
        private ILogger Log = Serilog.Log.ForContext<InternalSetParser>();
        //private JsonSerializer _jsonSerializer = new JsonSerializer();

        public bool IsCompatible(IParseInfo info)
        {
            if (Uri.TryCreate(info.URI, UriKind.Absolute, out var uri))
            {
                return uri.LocalPath.EndsWith(".r4uset");
            }
            else
            {
                return Fluent.IO.Path.Get(info.URI).Extension == ".r4uset";
            }
        }

        public async IAsyncEnumerable<R4UCard> Parse(string urlOrLocalFile)
        {
            Log.Information("Starting...");
            using (Stream s = await GetStreamFromURIOrFile(urlOrLocalFile))
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

                    foreach (var alt in card.Alternates ?? new R4UCard[] { })
                    {
                        alt.NonFoil = card;
                        alt.FillProxy();
                        yield return alt;
                    }
                }
            }
        }
        private static async Task<Stream> GetStreamFromURIOrFile(string urlOrLocalFile)
        {
            if (Uri.TryCreate(urlOrLocalFile, UriKind.Absolute, out var uri))
            {
                return await urlOrLocalFile.WithRESTHeaders().GetStreamAsync();
            }
            else
                return Fluent.IO.Path.Get(urlOrLocalFile).GetStream();
        }
    }

    public class InternalCardSet
    {
        public int Version { get; set; }
        public List<R4UCard> Cards { get; set; }
    }
}
