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
        [TestMethod("IQ/001T - Isekai Quartet Trial Start Deck Parse Test")]
        public async Task IQ001TTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Isekai_Quartet" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("IQ/001T-009[OL]"));
            }
        }

        [TestMethod("IQ/001B- Isekai Quartet BP Parse Test")]
        public async Task IQ001BTest()
        {
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Isekai_Quartet" }.Run(_ioc);
            using (var _db = _ioc.GetInstance<CardDatabaseContext>())
            {
                Assert.IsNotNull(await _db.R4UCards.FindAsync("IQ/001B-091SR＋[YS]"));
            }
        }
    }
}
