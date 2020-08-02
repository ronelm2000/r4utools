using Montage.RebirthForYou.Tools.CLI.Entities;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IDeckExporter
    {
        public string[] Alias { get; }
        public Task Export(R4UDeck deck, IExportInfo info);// destinationFolderOrURL);
    }
}
