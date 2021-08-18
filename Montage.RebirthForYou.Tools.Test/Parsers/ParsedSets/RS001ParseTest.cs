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
        [TestMethod("RS/SD - Revue Starlight -Re LIVE- Teaching Deck Parse Test")]
        public async Task RSSDTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Revue_Starlight_-Re_LIVE-%22" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("RS/SD-0009"));

            }
        }
        // https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Revue_Starlight_-Re_LIVE-%22
        [TestMethod("RS/001T - Revue Starlight -Re LIVE- Trial Start Deck Parse Test")]
        public async Task RS001TTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Revue_Starlight_-Re_LIVE-" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("RS/001T-007"));
            }
        }

        // https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-
        [TestMethod("RS/001B - Revue Starlight -Re LIVE- BP Parse Test")]
        public async Task RS001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("RS/001B-012"));
            }
        }
    }
}
