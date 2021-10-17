using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities.RebirthForYou
{
    internal static class R4URegex
    {
        public static readonly Regex ReleaseIDMatcher = new(@"(([A-Za-z0-9]+)(\/)([^-]+))-");

    }
}
