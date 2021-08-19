using Lamar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.Test.Parsers.ParsedSets
{
    [TestClass]
    [Ignore("Ignored during building as these can fail when being rate-limited.")]
    public partial class ParsedSetsTest
    {
        private static Container _ioc;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Serilog.Log.Logger = TestUtils.BootstrapLogging().CreateLogger();
            _ioc = Program.Bootstrap();
        }
    }
}
