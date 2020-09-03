using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Exporters.Deck
{
    /// <summary>
    /// Must be explicity stated as an exporter. Indicating this as an exporter means that there is no actual output.
    /// </summary>
    public class NullDeckExporter : IDeckExporter, IFilter<IExportedDeckInspector>
    {
        public string[] Alias => new[] { "null", "nil" };

        public Task Export(R4UDeck deck, IExportInfo info)
        {
            return Task.CompletedTask;
        }

        public bool IsIncluded(IExportedDeckInspector item)
        {
            return false;
        }
    }
}
