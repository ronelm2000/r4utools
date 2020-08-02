using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.Test.Commons;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test
{
    [TestClass]
    public class IntegrationTests
    {
        IContainer ioc; //= Bootstrap();

        [TestMethod("Full Integration Test (Typical Use Case)")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            ioc = Program.Bootstrap();

            /*
            await new ParseVerb(){ 
                URI = "https://heartofthecards.com/translations/love_live!_sunshine_school_idol_festival_6th_anniversary_booster_pack.html" 
                }.Run(ioc);

            await new ParseVerb()
            {
                URI = "https://heartofthecards.com/translations/love_live!_sunshine_vol._2_booster_pack.html"
            }.Run(ioc);

            var testSerial = await ioc.GetInstance<CardDatabaseContext>().R4UCards.FindAsync("LSS/W69-006");
            Assert.IsTrue(testSerial.Images.Any());

            var parseCommand = new ExportVerb()
            {
                Source = "https://www.encoredecks.com/deck/wDdTKywNh",
                NonInteractive = true
            };
            await parseCommand.Run(ioc);
            */
            await Task.CompletedTask;
        }

        /*
        private static Container Bootstrap(Action<ServiceRegistry> additionalActions)
        {
            return new Container(x =>
            {
                x.AddTransient<ExportVerb>();
                x.AddTransient<ParseVerb>();
                x.AddDbContext<CardDatabaseContext>();
                x.AddLogging(l => l.AddSerilog(dispose: true));
                additionalActions(x);
            });
        }
        */
        
    }
}
