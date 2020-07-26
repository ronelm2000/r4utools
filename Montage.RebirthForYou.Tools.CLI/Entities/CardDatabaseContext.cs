﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        }

        internal async Task<R4UCard> FindNonFoil(R4UCard card)
        {
            return await R4UCards.FindAsync(R4UCard.RemoveFoil(card.Serial));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<R4UCard>(b =>
            {
                b   .HasKey(c => c.Serial);
                b   .Property(c => c.Triggers)
                    .HasConversion( arr => String.Join(',', arr.Select(t => t.ToString()))
                                ,   str => str.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToEnum<Trigger>().Value).ToArray()
                            );
                b   .Property(c => c.Effect)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr)
                                ,   str => JsonConvert.DeserializeObject<string[]>(str)
                                    );
                b   .Property(c => c.Images)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr.Select(uri => uri.ToString()).ToArray())
                                ,   str => JsonConvert.DeserializeObject<string[]>(str).Select(s => new Uri(s)).ToList()
                                    );
                
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
                        
           // modelBuilder.Entity<MultiLanguageString>().HasKey(s => s.JP);

        }
    }
}
