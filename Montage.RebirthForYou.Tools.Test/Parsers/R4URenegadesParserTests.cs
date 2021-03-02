using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.Parsers
{
    [TestClass]
    public class R4URenegadesParserTests
    {

        [TestMethod("Full Integration Test (Fandom Wiki) (Typical Use Case)")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            await new ParseVerb { URI = "https://rebirthforyourenegades.wordpress.com/2020/12/23/th-001e-full-set-list/" }.Run(ioc);
            await new ParseVerb { URI = "https://rebirthforyourenegades.wordpress.com/2021/02/24/imc-001b-full-set-list/" }.Run(ioc);
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                var chieri = await db.R4UCards.FindAsync("IMC/001B-015");
                Assert.IsTrue(chieri.Name.JP == "緒方 智絵里");
                Assert.IsTrue(chieri.Name.EN == "Chieri Ogata");
                Assert.IsTrue(chieri.Traits.Any(t => t.EN == "Cute"));
                Assert.IsTrue(chieri.ATK == 2);
                Assert.IsTrue(chieri.DEF == 3);
                Assert.IsTrue(chieri.Effect.Any(e => e.EN == "[AUTO]:When this character is put onto your member area from hand, choose a character each from your retire area and hand, and you may exchange their positions."));

            }
        }
    }
}
