using System.Collections.Generic;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IParseInfo
    {
        string URI { get; }
        IEnumerable<string> ParserHints { get; }
    }
}