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
        [TestMethod("GP/SD - Bang Dream Girls Band Party PICO Teaching Deck Parse Test")]
        public async Task GPSDTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22BanG_Dream!_Girls_Band_Party!%E2%98%86PICO%22" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("GP/SD-0009"));
            }
        }

        [TestMethod("GP/001T - Bang Dream Girls Band Party PICO Trial Deck Parse Test")]
        public async Task GP001TTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("GP/001T-010"));
            }
        }

        [TestMethod("GP/001B - Bang Dream Girls Band Party PICO BP Parse Test")]
        public async Task GP001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("GP/001B-010"));
            }
        }
    }
}
