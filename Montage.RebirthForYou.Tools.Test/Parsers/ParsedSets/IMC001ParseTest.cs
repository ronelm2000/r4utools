using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.RebirthForYou.Tools.CLI;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Montage.RebirthForYou.Tools.Test.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Montage.RebirthForYou.Tools.Test.Parsers.ParsedSets
{
    public partial class ParsedSetsTest
    {
        [TestMethod("IMC/001B - IM@S Cinderella Girls BP Parse Test (Renegades)")]
        public async Task IMC001BTest()
        {
            await new ParseVerb { URI = "https://rebirthforyourenegades.wordpress.com/2021/02/24/imc-001b-full-set-list/" }.Run(_ioc);
        }
    }
}
