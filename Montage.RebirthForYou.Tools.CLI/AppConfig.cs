using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI
{
    public class AppConfig
    {
        public string DbName { get; set; } = "cards.db";
        public bool EnableDbDebug { get; set; } = false;
    }
}
