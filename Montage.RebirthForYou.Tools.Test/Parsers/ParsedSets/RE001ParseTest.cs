using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI.CLI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers.ParsedSets
{
    public partial class ParsedSetsTest
    {
        [TestMethod("RE/SD - Rebirth 4 You! Teaching Deck Parse Test")]
        public async Task HPSDTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22" }.Run(_ioc);
        }
    }
}
