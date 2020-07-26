using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    interface IExactCloneable<T>
    {
        public T Clone();
    }
}
