using CommandLine;

namespace Montage.RebirthForYou.Tools.CLI
{
    internal class Options
    {
        [Value(0)]
        public string URI { get; set; }
    }
}