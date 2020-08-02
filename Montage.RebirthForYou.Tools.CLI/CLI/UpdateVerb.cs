using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.CLI
{
    [Verb("update", HelpText = "Updates the database using the present Activity Log.")]
    public class UpdateVerb : IVerbCommand
    {
        private ILogger Log = Serilog.Log.ForContext<UpdateVerb>();

        public async Task Run(IContainer ioc)
        {
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                Log.Information("Updating Database Using Activity Log Queue...");
                var activityLog = db.MigrationLog.AsQueryable()
                    .Where(log => !log.IsDone)
                    .OrderBy(log => log.DateAdded)
                    ;
                await foreach (var act in activityLog.AsAsyncEnumerable())
                {
                    await act.ToCommand().Run(ioc);
                    act.IsDone = true;
                }
                Log.Information("Done!");
            }
        }
    }
}
