using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.API
{
    public interface IDatabaseExporter
    {
        public string[] Alias { get; }
        public Task Export(CardDatabaseContext database, IDatabaseExportInfo info);
    }
}
