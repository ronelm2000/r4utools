using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers
{
    [TestClass]
    public class FandomWikiParserTests
    {
        [TestMethod("Full Integration Test (Fandom Wiki) (Typical Use Case)")]
        [Ignore]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            await new ParseVerb { URI = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Is_the_Order_a_Rabbit%3F_BLOOM" }.Run(ioc);
        }
    }
}
