using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class CardDatabaseContextDesignFactory : IDesignTimeDbContextFactory<CardDatabaseContext>
    {
        /*
        public BloggingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BloggingContext>();
            optionsBuilder.UseSqlite("Data Source=blog.db");

            return new BloggingContext(optionsBuilder.Options);
        }
        */

        CardDatabaseContext IDesignTimeDbContextFactory<CardDatabaseContext>.CreateDbContext(string[] args)
        {
            return new CardDatabaseContext();
        }
    }
}
