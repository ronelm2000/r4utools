using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck
{
    public class DefaultDeckParser : IDeckParser
    {
        public string[] Alias => new string[] { };

        public int Priority => int.MinValue;

        public bool IsCompatible(string urlOrFile)
        {
            return true;
        }

        private readonly ILogger Log = Serilog.Log.ForContext<DefaultDeckParser>();

        public Task<R4UDeck> Parse(string sourceUrlOrFile)
        {
            Log.Error("Cannot find a compatible parser for this URL or File: {file}", sourceUrlOrFile);
            throw new NotImplementedException();
        }
    }
}
