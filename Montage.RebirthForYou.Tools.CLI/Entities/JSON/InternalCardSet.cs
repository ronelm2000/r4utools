using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Entities.JSON
{
    public class InternalCardSet
    {
        public int Version { get; set; }
        public List<R4UCard> Cards { get; set; }
    }
}
