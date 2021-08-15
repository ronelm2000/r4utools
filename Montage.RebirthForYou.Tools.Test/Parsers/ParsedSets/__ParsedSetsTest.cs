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
