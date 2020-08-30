using Lamar;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.CLI;
using Octokit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public static class ActivityExtensions
    {
        public static string ToVerbString(this ActivityType actType)
        {
            return actType switch
            {
                ActivityType.Parse => "Parsing",
                _ => throw new NotImplementedException()
            };
        }
    }
}