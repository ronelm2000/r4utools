using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IDeckParser
    {
        public string[] Alias { get; }
        public int Priority { get; }
        public bool IsCompatible(string urlOrFile);
        public Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile);
    }
}
