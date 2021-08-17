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
        public async Task RESDTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22" }.Run(_ioc);
        }

        [TestMethod("RE/001B - Rebirth 4 You! BP Parse Test")]
        public async Task RE001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Rebirth" }.Run(_ioc);
        }

    }
}
