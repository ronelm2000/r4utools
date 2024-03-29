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
        private readonly AppConfig _config = new() { DbName = "cards.db" };
        private readonly ILogger Log = Serilog.Log.ForContext<CardDatabaseContext>();

        public DbSet<R4UCard> R4UCards { get; set; }
        public DbSet<R4UReleaseSet> R4UReleaseSets { get; set; }
        public DbSet<ActivityLog> MigrationLog { get; set; }
        public DbSet<Setting> Settings { get; set; }
        //public DbSet<MultiLanguageString> MultiLanguageStrings { get; set; }


        public CardDatabaseContext (AppConfig config) {
            Log.Debug("Instantiating with {@AppConfig}.", config);
            _config = config;
        }

        public CardDatabaseContext()
        {
            try
            {
                Log.Debug("Instantiating with no arguments.");
                using StreamReader file = File.OpenText(@"app.json");
                using JsonTextReader reader = new(file);
                _config = JToken.ReadFrom(reader).ToObject<AppConfig>();
            }
            catch (Exception)
            {
            }
        }

        /*
        public CardDatabaseContext CreateDbContext(string[] args)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<CardDatabaseContext>();
            //optionsBuilder.UseSqlite("Data Source=blog.db");
            return new CardDatabaseContext();// (optionsBuilder.Options);
        }
        */

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={_config.DbName}");
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            if (_config.EnableDbDebug)
                options.LogTo(Log.Debug, Microsoft.Extensions.Logging.LogLevel.Debug);
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
                b.HasOne(c => c.NonFoil).WithMany(c => c.Alternates).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(c => c.Alternates).WithOne(c => c.NonFoil);
                b.OwnsMany(s => s.Traits, bb =>
                {
                    bb.Property<int>("Id").HasAnnotation("Sqlite:Autoincrement", true);
                    bb.HasKey("Id");        
                    bb  .WithOwner()
                        .HasPrincipalKey(s => s.Serial);
                });

                b   .OwnsOne(c => c.Name, bb =>
                {
                    bb.WithOwner();
                });
            });

            modelBuilder.Entity<R4UReleaseSet>(b =>
            {
                b.HasKey(s => s.ReleaseID);
                b.HasMany(s => s.Cards).WithOne(c => c.Set).OnDelete(DeleteBehavior.Cascade);
                b.Navigation(s => s.Cards)
                    .UsePropertyAccessMode(PropertyAccessMode.Property);
                b.Property(s => s.ReleaseCode).IsRequired();
                b.Property(s => s.Name).IsRequired(false);
                b.Property(s => s.Description).IsRequired(false);
            });

            modelBuilder.Entity<Setting>(b =>
            {
                b.HasKey(s => s.Key);
            });

            modelBuilder.Entity<ActivityLog>(b =>
            {
                var index = 0;
                b.HasKey(a => a.LogID);
                b.HasData(
                    new ActivityLog
                    {
                        LogID = 1,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/gochiusa_bp.r4uset",
                        IsDone = true,
                        DateAdded = new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446)
                    },
                    new ActivityLog
                    {
                        LogID = 2,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Is_the_Order_a_Rabbit%3F_BLOOM",
                        IsDone = true,
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
                        DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 205, DateTimeKind.Local),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 6,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)",
                        DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 206, DateTimeKind.Local),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 7,
                        Activity = ActivityType.Parse,
                        IsDone = true,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_%28ver._2nd_Gen%29",
                        DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 8,
                        Activity = ActivityType.Parse,
                        IsDone = true,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._GAMERS)",
                        DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local)
                    },
                    new ActivityLog
                    {
                        LogID = 9,
                        Activity = ActivityType.Parse,
                        IsDone = true,
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
                        DateAdded = new DateTime(2020, 10, 26, 0, 0, 3),
                        IsDone = true,
                    },
                    new ActivityLog
                    {
                        LogID = 12,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Promo_Cards",
                        DateAdded = new DateTime(2020, 11, 03, 0, 0, 1),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 13,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Azur_Lane",
                        DateAdded = new DateTime(2020, 12, 21, 0, 0, 4),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 14,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production",
                        IsDone = true,
                        DateAdded = new DateTime(2020, 12, 21, 0, 0, 5)
                    },
                    new ActivityLog
                    {
                        LogID = 15,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22",
                        IsDone = true,
                        DateAdded = new DateTime(2021, 01, 02, 0, 0, 7, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 16,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22BanG_Dream!_Girls_Band_Party!%E2%98%86PICO%22",
                        IsDone = true,
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
                        IsDone = true,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset",
                        DateAdded = new DateTime(2021, 08, 20, 18, 34, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 19,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Rebirth",
                        IsDone = true,
                        DateAdded = new DateTime(2020, 01, 02, 0, 0, 11)
                    },
                    new ActivityLog
                    {
                        LogID = 20,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO",
                        IsDone = true,
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
                        IsDone = true,
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
                        IsDone = true,
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 15, 0, DateTimeKind.Unspecified)
                    },
                    new ActivityLog
                    {
                        LogID = 25,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO",
                        IsDone = true,
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
                        IsDone = true,
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 18)
                    },
                    new ActivityLog
                    {
                        LogID = 28,
                        Activity = ActivityType.Parse,
                        Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-",
                        DateAdded = new DateTime(2021, 01, 02, 1, 0, 19)
                    },
                    new ActivityLog
                    {
                        LogID = 29,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_002B.r4uset",
                        IsDone = true,
                        DateAdded = new DateTime(2021, 08, 20, 18, 12, 19)
                    },
                    new ActivityLog
                    {
                        LogID = 30,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_IMC_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 20, 18, 12, 20)
                    },
                    new ActivityLog
                    {
                        LogID = 31,
                        Activity = ActivityType.Parse,
                        IsDone = true,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset",
                        DateAdded = new DateTime(2021, 08, 20, 18, 34, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 32,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_IMC_001T.r4uset",
                        DateAdded = new DateTime(2021, 08, 20, 18, 34, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 33,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_001T.r4uset",
                        DateAdded = new DateTime(2021, 08, 21, 0, 15, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 34,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_002T.r4uset",
                        DateAdded = new DateTime(2021, 08, 21, 0, 15, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 35,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001E.r4uset",
                        DateAdded = new DateTime(2021, 08, 21, 0, 15, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 36,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_003T.r4uset",
                        DateAdded = new DateTime(2021, 08, 21, 0, 15, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 37,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_001E.r4uset",
                        DateAdded = new DateTime(2021, 08, 21, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 38,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 39,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_RZ_001T.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 40,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_001T.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 41,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/gochiusa_bp.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 42,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001T.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 43,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_RZ_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 44,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 13, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 45,
                        Activity = ActivityType.Parse,
                        IsDone = true,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 23, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 46,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001B.r4uset",
                        IsDone = true,
                        DateAdded = new DateTime(2021, 08, 28, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 47,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001E.r4uset",
                        DateAdded = new DateTime(2021, 08, 28, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 48,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 28, 15, 16, 0),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 49,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001B.r4uset",
                        DateAdded = new DateTime(2021, 08, 31, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 50,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_002T.r4uset",
                        DateAdded = new DateTime(2021, 08, 31, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 51,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_004T.r4uset",
                        DateAdded = new DateTime(2021, 08, 31, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 52,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001B.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 13, 35, 0),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 53,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_001B.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 16, 35, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 54,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_001T.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 16, 35, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 55,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_002B.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 16, 35, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 56,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_SD.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 16, 35, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 57,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001B.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 20, 00, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 58,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001T.r4uset",
                        DateAdded = new DateTime(2021, 09, 03, 20, 00, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 59,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_003T.r4uset",
                        DateAdded = new DateTime(2021, 09, 20, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 60,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_Re_SD.r4uset",
                        DateAdded = new DateTime(2021, 09, 20, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 61,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001T.r4uset",
                        DateAdded = new DateTime(2021, 09, 20, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 62,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_Re_001T.r4uset",
                        DateAdded = new DateTime(2021, 09, 20, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 63,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_Re_001B.r4uset",
                        DateAdded = new DateTime(2021, 09, 20, 15, 16, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 64,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset",
                        DateAdded = new DateTime(2021, 09, 26, 15, 16, 0),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 65,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_RE_002B.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 20, 35, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 66,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_005T.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 20, 47, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 67,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_002T.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 20, 56, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 68,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GZ_001T.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 21, 00, 0),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 69, // nice
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_001B.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 21, 09, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 70,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GZ_001B.r4uset",
                        DateAdded = new DateTime(2021, 10, 11, 21, 56, 0),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 71,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GZ_001B.r4uset",
                        DateAdded = new DateTime(2021, 10, 17, 16, 29, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 72,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GZ_001T.r4uset",
                        DateAdded = new DateTime(2021, 10, 17, 16, 29, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 73,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_001E.r4uset",
                        DateAdded = new DateTime(2021, 10, 18, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 74,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_AL_001B.r4uset",
                        DateAdded = new DateTime(2021, 10, 18, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 75,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_AL_001T.r4uset",
                        DateAdded = new DateTime(2021, 10, 18, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 76,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001B.r4uset",
                        DateAdded = new DateTime(2021, 10, 18, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 77,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001T.r4uset",
                        DateAdded = new DateTime(2021, 10, 18, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 78,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_002B.r4uset",
                        DateAdded = new DateTime(2022, 04, 14, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 79,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_AL_002B.r4uset",
                        DateAdded = new DateTime(2022, 04, 14, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 80,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_002E.r4uset",
                        DateAdded = new DateTime(2022, 04, 14, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 81,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BKRM_001B.r4uset",
                        DateAdded = new DateTime(2022, 04, 14, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 82,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BKRM_001T.r4uset",
                        DateAdded = new DateTime(2022, 04, 14, 14, 26, 0)
                    },
                    new ActivityLog
                    {
                        LogID = 83,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BA_001B.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 84,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BA_001T.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 85,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_002B.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 86,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_003E.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 87,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGND_001B.r4uset",
                        DateAdded = new DateTime(638365563916606059L),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 88,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGND_001T.r4uset",
                        DateAdded = new DateTime(638365563916606059L),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 89,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_001B.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 90,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_001T.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 91,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_STD_001B.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 92,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_STD_001TV.r4uset",
                        DateAdded = new DateTime(638365563916606059L)
                    },
                    new ActivityLog
                    {
                        LogID = 93,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_001TV.r4uset",
                        DateAdded = new DateTime(638366221721891022L),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 94,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_002B.r4uset",
                        DateAdded = new DateTime(638366221721891022L)
                    },
                    new ActivityLog
                    {
                        LogID = 95,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_YC_001B.r4uset",
                        DateAdded = new DateTime(638366221721891022L)
                    },
                    new ActivityLog
                    {
                        LogID = 96,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_YC_001T.r4uset",
                        DateAdded = new DateTime(638366221721891022L)
                    },
                    new ActivityLog
                    {
                        LogID = 97,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_001C.r4uset",
                        DateAdded = new DateTime(638366379629021107L)
                    },
                    new ActivityLog
                    {
                        LogID = 98,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HS_001B.r4uset",
                        DateAdded = new DateTime(638366379629021107L)
                    },
                    new ActivityLog
                    {
                        LogID = 99,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HS_001T.r4uset",
                        DateAdded = new DateTime(638366379629021107L)
                    },
                    new ActivityLog
                    {
                        LogID = 100,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_007T.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 101,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGND_001B.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 102,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGND_001T.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 103,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KJ_001T.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 104,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KJ_001B.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 105,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_LR_001B.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 106,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_LR_001T.r4uset",
                        DateAdded = new DateTime(638368671599361770L)
                    },
                    new ActivityLog
                    {
                        LogID = 107,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_002B.r4uset",
                        DateAdded = new DateTime(638368796020821339L)
                    },
                    new ActivityLog
                    {
                        LogID = 108,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_002B.r4uset",
                        DateAdded = new DateTime(638368796020821339L),
                        IsDone = true
                    },
                    new ActivityLog
                    {
                        LogID = 109,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_FG_001B.r4uset",
                        DateAdded = new DateTime(638368796020821339L)
                    },
                    new ActivityLog
                    {
                        LogID = 110,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_FG_001T.r4uset",
                        DateAdded = new DateTime(638368796020821339L)
                    },
                    new ActivityLog
                    {
                        LogID = 111,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGJT_001B.r4uset",
                        DateAdded = new DateTime(638368796020821339L)
                    },
                    new ActivityLog
                    {
                        LogID = 112,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KGJT_001T.r4uset",
                        DateAdded = new DateTime(638368796020821339L)
                    },
                    new ActivityLog
                    {
                        LogID = 113,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_001P.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 114,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_STD_001P.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 115,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TOP_001T.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 116,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TOP_001B.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 117,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TS_001T.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 118,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TS_001B.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 119,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_BA_002B.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 120,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_002B.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 121,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_LR_001E.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 122,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_002B.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 123,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_KS_002T.r4uset",
                        DateAdded = new DateTime(638369657986655019L)
                    },
                    new ActivityLog
                    {
                        LogID = 124,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_NJPW_001TV.r4uset",
                        DateAdded = new DateTime(638391989296113309L),
                    },
                    new ActivityLog
                    {
                        LogID = 125,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_002B.r4uset",
                        DateAdded = new DateTime(638391989296113309L)
                    },
                    new ActivityLog
                    {
                        LogID = 126,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset",
                        DateAdded = new DateTime(638391989296113309L)
                    },
                    new ActivityLog
                    {
                        LogID = 127,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_001T.r4uset",
                        DateAdded = new DateTime(638391989296113309L)
                    },
                    new ActivityLog
                    {
                        LogID = 128,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_U149_001T.r4uset",
                        DateAdded = new DateTime(638398373920331112L)
                    },
                    new ActivityLog
                    {
                        LogID = 129,
                        Activity = ActivityType.Parse,
                        Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_U149_001B.r4uset",
                        DateAdded = new DateTime(638398373920331112L)
                    }
                );
            });
        }
    }
}
