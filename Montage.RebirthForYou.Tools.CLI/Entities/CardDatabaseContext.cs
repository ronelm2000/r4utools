using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class CardDatabaseContext : DbContext
    {
        private readonly AppConfig _config;
        private readonly ILogger Log = Serilog.Log.ForContext<CardDatabaseContext>();

        public DbSet<R4UCard> R4UCards { get; set; }
        public DbSet<R4UReleaseSet> R4UReleaseSets { get; set; }
        public DbSet<ActivityLog> MigrationLog { get; set; }
        //public DbSet<MultiLanguageString> MultiLanguageStrings { get; set; }

        public CardDatabaseContext (AppConfig config) {
            Log.Debug("Instantiating with {@AppConfig}.", config);
            _config = config;
        }

        public CardDatabaseContext()
        {
            Log.Debug("Instantiating with no arguments.");
            using (StreamReader file = File.OpenText(@"app.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                _config = JToken.ReadFrom(reader).ToObject<AppConfig>();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={_config.DbName}");
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }

        internal Task<R4UCard> FindNonFoil(R4UCard card)
        {
            return Task.FromResult(((card.IsFoil) ? card.Alternates.FirstOrDefault() : card) ?? card);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<R4UCard>(b =>
            {
                b   .HasKey(c => c.Serial).HasName("Serial");
                b   .Property(c => c.Effect)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr)
                                ,   str => JsonConvert.DeserializeObject<MultiLanguageString[]>(str)
                                    );
                b   .Property(c => c.Images)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr.Select(uri => uri.ToString()).ToArray())
                                ,   str => JsonConvert.DeserializeObject<string[]>(str).Select(s => new Uri(s)).ToList()
                                    );
                b   .Property(c => c.Flavor)
                    .HasConversion( flavor => JsonConvert.SerializeObject(flavor)
                                ,   str => JsonConvert.DeserializeObject<MultiLanguageString>(str)
                                    );
                //b.Property(c => c.Alternates);
                b.HasOne(c => c.NonFoil).WithMany(c => c.Alternates);
                b.HasMany(c => c.Alternates).WithOne(c => c.NonFoil);

                b.OwnsMany(s => s.Traits, bb =>
                 {
                     bb.Property<int>("Id").HasAnnotation("Sqlite:Autoincrement", true);
                     bb.HasKey("Id");
                     bb.WithOwner().HasPrincipalKey(s => s.Serial);
                 });

                b   .OwnsOne(c => c.Name, bb =>
                {
                    bb.WithOwner();
                });
            });

            modelBuilder.Entity<R4UReleaseSet>(b =>
            {
                b.HasKey(s => s.ReleaseID);
                b.HasMany(s => s.Cards).WithOne(c => c.Set);
            });

            modelBuilder.Entity<ActivityLog>(b =>
            {
                b.HasKey(a => a.LogID);
            });
           // modelBuilder.Entity<MultiLanguageString>().HasKey(s => s.JP);

        }
    }
}
