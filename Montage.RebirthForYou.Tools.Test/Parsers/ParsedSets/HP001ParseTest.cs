using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers.ParsedSets
{
    public partial class ParsedSetsTest
    {
        [TestMethod("HP/001B - Hololive Production BP Parse Test")]
        public async Task HP001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("HP/001B-013"));
            }
        }
    }
}
