using Fluent.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.Test.LocalJSON
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod("Full Integration Test (Local JSON) (Typical Use Case)")]
        [DeploymentItem("Resources/sample.r4uset")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            Lamar.Container ioc = Program.Bootstrap();

            await Task.CompletedTask;
            /*
            await new ParseVerb()
            {
                URI = "https://www.encoredecks.com/?page=1&set=5cfbffe67cd9b718cdf4b439"
            }.Run(ioc);

            await new ExportVerb()
            {
                Source = "./deck_date_a_live.json",
                Exporter = "local",
                NonInteractive = true,
                NoWarning = true
            }.Run(ioc);

            Assert.IsTrue(Path.Get("./Export/deck_date_a_live.json").Exists);
            */
        }
    }
}
