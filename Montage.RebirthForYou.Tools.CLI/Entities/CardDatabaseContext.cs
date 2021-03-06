﻿using Microsoft.EntityFrameworkCore;
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
                b.HasData(
                    new ActivityLog
                    {
                        LogID = 1,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/gochiusa_bp.r4uset",
                        DateAdded = new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446)
                    },
                    new ActivityLog
                    {
                        LogID = 2,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Is_the_Order_a_Rabbit%3F_BLOOM",
                        DateAdded = new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446)
                    },
                    new ActivityLog
                    {
                        LogID = 3,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)",
                        DateAdded = new DateTime(2020, 8, 23, 0, 43, 53, 205, DateTimeKind.Local),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 4,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)",
                        DateAdded = new DateTime(2020, 8, 23, 0, 43, 53, 206, DateTimeKind.Local),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 5,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)",
                        DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 205, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 6,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)",
                        DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 206, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 7,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_%28ver._2nd_Gen%29",
                        DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 8,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._GAMERS)",
                        DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 9,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._3rd_Gen)",
                        DateAdded = new DateTime(2020, 10, 26, 0, 0, 1)
                    },
                    new ActivityLog
                    {
                        LogID = 10,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._4th_Gen)",
                        DateAdded = new DateTime(2020, 10, 26, 0, 0, 2)
                    },
                    new ActivityLog
                    {
                        LogID = 11,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Azur_Lane",
                        DateAdded = new DateTime(2020, 10, 26, 0, 0, 3)
                    },
                    new ActivityLog
                    {
                        LogID = 12,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Promo_Cards",
                        DateAdded = new DateTime(2020, 11, 03, 0, 0, 1)
                    },
                    new ActivityLog
                    {
                        LogID = 13,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Azur_Lane",
                        DateAdded = new DateTime(2020, 12, 21, 0, 0, 4)
                    },
                    new ActivityLog
                    {
                        LogID = 14,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production",
                        DateAdded = new DateTime(2020, 12, 21, 0, 0, 5)
                    },
                    new ActivityLog
                    {
                        LogID = 15,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22",
                        DateAdded = new DateTime(2021, 01, 02, 0, 0, 7, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 16,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22BanG_Dream!_Girls_Band_Party!%E2%98%86PICO%22",
                        DateAdded = new DateTime(2021, 01, 02, 0, 0, 8, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 17,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Revue_Starlight_-Re_LIVE-%22",
                        DateAdded = new DateTime(2021, 01, 02, 0, 0, 9, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 18,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset",
                        DateAdded = new DateTime(2020, 01, 02, 0, 0, 10)
                    },
                    new ActivityLog
                    {
                        LogID = 19,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Rebirth",
                        DateAdded = new DateTime(2020, 01, 02, 0, 0, 11)
                    },
                    new ActivityLog
                    {
                        LogID = 20,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO",
                        DateAdded = new DateTime(2021, 01, 02, 0, 0, 12, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 21,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Isekai_Quartet",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 12, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 22,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Touhou_Project",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 13, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 23,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Revue_Starlight_-Re_LIVE-",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 14, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 24,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Rebirth",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 15, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 25,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 16, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 26,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Isekai_Quartet",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 17)
                    },
                    new ActivityLog
                    {
                        LogID = 27,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Touhou_Project",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 18)
                    },
                    new ActivityLog
                    {
                        LogID = 28,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 19)
                    }
                );
            });
        }
    }
}
