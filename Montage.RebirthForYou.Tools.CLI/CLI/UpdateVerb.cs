using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.CLI
{
    [Verb("update", HelpText = "Updates the database using the present Activity Log.")]
    public class UpdateVerb : IVerbCommand
    {
        public async Task Run(IContainer ioc)
        {
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                var activityLog = db.MigrationLog.AsQueryable()
                    .Where(log => !log.IsDone)
                    .OrderBy(log => log.DateAdded)
                    .Distinct(ActivityLog.ByCommand)
                    ;
            }
            throw new NotImplementedException();
        }
    }
}
