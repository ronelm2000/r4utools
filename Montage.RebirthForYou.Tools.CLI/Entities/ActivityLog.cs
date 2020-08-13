using Lamar;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class ActivityLog
    {
        public int LogID { get; set; }
        public ActivityType Activity { get; set; }
        public string Target { get; set; }
        public bool IsDone { get; set; } = false;
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public IVerbCommand ToCommand()
        {
            return Activity switch
            {
                ActivityType.Parse => new ParseVerb { URI = Target },
                _ => throw new NotImplementedException()
            };
        }
    }

    public enum ActivityType
    {
        Parse = 0
    }
}