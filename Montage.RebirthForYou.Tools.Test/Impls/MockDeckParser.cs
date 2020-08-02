using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Impls
{
    class MockDeckParser : IDeckParser
    {
        public string[] Alias => new[] { "mock", "" };

        public int Priority => int.MaxValue;

        public bool IsCompatible(string urlOrFile)
        {
            return true;
        }

        public Task<R4UDeck> Parse(string sourceUrlOrFile)
        {
            return null;
        }
    }
}
