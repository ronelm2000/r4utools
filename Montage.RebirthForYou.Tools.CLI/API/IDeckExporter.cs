using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.Weiss.Tools.CLI;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IDeckExporter
    {
        public string[] Alias { get; }
        public Task Export(WeissSchwarzDeck deck, IExportInfo info);// destinationFolderOrURL);

    }
}
