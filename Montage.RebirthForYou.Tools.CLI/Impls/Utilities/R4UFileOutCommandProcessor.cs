using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Impls.Utilities
{
    public class R4UFileOutCommandProcessor : FileOutCommandProcessor
    {
        public override ILogger Log => Serilog.Log.ForContext<R4UFileOutCommandProcessor>();
    }
}
