using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers.ParsedSets
{
    public partial class ParsedSetsTest
    {
        [TestMethod("TH/001T - Touhou Project Start Trial Deck Parse Test")]
        public async Task TH001TTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Touhou_Project" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("TH/001T-007"));

            }
        }

        [TestMethod("TH/001B - Touhou Project BP Parse Test")]
        public async Task TH001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Touhou_Project" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("TH/001B-089"));
            }
        }
    }
}
