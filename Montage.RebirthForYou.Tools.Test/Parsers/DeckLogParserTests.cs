using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Impls.Parsers.Deck;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers
{
    [TestClass]
    public class DeckLogParserTests
    {
        [TestMethod("Full Integration Test (Deck Log Deck Parser) (Typical Use Case)")]
        public async Task TestFullIntegrationTest()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)" }.Run(ioc);
            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)" }.Run(ioc);
            var testSerial = await ioc.GetInstance<CardDatabaseContext>().R4UCards.FindAsync("HP/001T-013");
            var deck = await ioc.GetInstance<DeckLogParser>().Parse("https://decklog.bushiroad.com/view/3H0N");
            
        }
    }
}
