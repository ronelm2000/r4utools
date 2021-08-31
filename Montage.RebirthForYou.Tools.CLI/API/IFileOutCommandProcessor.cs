using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IFileOutCommandProcessor
    {
        public Task Process(string fullOutCommand, string fullFilePath);
    }
}