﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Montage.RebirthForYou.Tools.CLI.Entities;

namespace Montage.RebirthForYou.Tools.CLI.Migrations
{
    [DbContext(typeof(CardDatabaseContext))]
    partial class CardDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("Montage.RebirthForYou.Tools.CLI.Entities.ActivityLog", b =>
                {
                    b.Property<int>("LogID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Activity")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDone")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Target")
                        .HasColumnType("TEXT");

                    b.HasKey("LogID");

                    b.ToTable("MigrationLog");

                    b.HasData(
                        new
                        {
                            LogID = 1,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446),
                            IsDone = true,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/gochiusa_bp.r4uset"
                        },
                        new
                        {
                            LogID = 2,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 2, 0, 0, 23, 534, DateTimeKind.Local).AddTicks(9446),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Is_the_Order_a_Rabbit%3F_BLOOM"
                        },
                        new
                        {
                            LogID = 3,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 8, 23, 0, 43, 53, 205, DateTimeKind.Local),
                            IsDone = true,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)"
                        },
                        new
                        {
                            LogID = 4,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 8, 23, 0, 43, 53, 206, DateTimeKind.Local),
                            IsDone = true,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)"
                        },
                        new
                        {
                            LogID = 5,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 205, DateTimeKind.Local),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._0th_Gen)"
                        },
                        new
                        {
                            LogID = 6,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 4, 0, 43, 53, 206, DateTimeKind.Local),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._1st_Gen)"
                        },
                        new
                        {
                            LogID = 7,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_%28ver._2nd_Gen%29"
                        },
                        new
                        {
                            LogID = 8,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 9, 25, 0, 43, 53, 206, DateTimeKind.Local),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._GAMERS)"
                        },
                        new
                        {
                            LogID = 9,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 10, 26, 0, 0, 1, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._3rd_Gen)"
                        },
                        new
                        {
                            LogID = 10,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 10, 26, 0, 0, 2, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Hololive_Production_(ver._4th_Gen)"
                        },
                        new
                        {
                            LogID = 11,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 10, 26, 0, 0, 3, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Deck_Azur_Lane"
                        },
                        new
                        {
                            LogID = 12,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 11, 3, 0, 0, 1, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Promo_Cards"
                        },
                        new
                        {
                            LogID = 13,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 12, 21, 0, 0, 4, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Azur_Lane"
                        },
                        new
                        {
                            LogID = 14,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 12, 21, 0, 0, 5, 0, DateTimeKind.Unspecified),
                            IsDone = true,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Hololive_Production"
                        },
                        new
                        {
                            LogID = 15,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 0, 0, 7, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Rebirth%22"
                        },
                        new
                        {
                            LogID = 16,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 0, 0, 8, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22BanG_Dream!_Girls_Band_Party!%E2%98%86PICO%22"
                        },
                        new
                        {
                            LogID = 17,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 0, 0, 9, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Teaching_Deck_%22Revue_Starlight_-Re_LIVE-%22"
                        },
                        new
                        {
                            LogID = 18,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 20, 18, 34, 0, 0, DateTimeKind.Unspecified),
                            IsDone = true,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset"
                        },
                        new
                        {
                            LogID = 19,
                            Activity = 0,
                            DateAdded = new DateTime(2020, 1, 2, 0, 0, 11, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Rebirth"
                        },
                        new
                        {
                            LogID = 20,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 0, 0, 12, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO"
                        },
                        new
                        {
                            LogID = 21,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 12, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Isekai_Quartet"
                        },
                        new
                        {
                            LogID = 22,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 13, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Touhou_Project"
                        },
                        new
                        {
                            LogID = 23,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 14, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Trial_Start_Deck_Revue_Starlight_-Re_LIVE-"
                        },
                        new
                        {
                            LogID = 24,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 15, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Rebirth"
                        },
                        new
                        {
                            LogID = 25,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 16, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_BanG_Dream!_Girls_Band_Party!%E2%98%86PICO"
                        },
                        new
                        {
                            LogID = 26,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 17, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Isekai_Quartet"
                        },
                        new
                        {
                            LogID = 27,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 18, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Touhou_Project"
                        },
                        new
                        {
                            LogID = 28,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 1, 2, 1, 0, 19, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://rebirth-for-you.fandom.com/wiki/Booster_Pack_Revue_Starlight_-Re_LIVE-"
                        },
                        new
                        {
                            LogID = 29,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 20, 18, 12, 19, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GP_002B.r4uset"
                        },
                        new
                        {
                            LogID = 30,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 20, 18, 12, 20, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_IMC_001B.r4uset"
                        },
                        new
                        {
                            LogID = 31,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 20, 18, 34, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/PR.r4uset"
                        },
                        new
                        {
                            LogID = 32,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 20, 18, 34, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_IMC_001T.r4uset"
                        },
                        new
                        {
                            LogID = 33,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 21, 0, 15, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_001T.r4uset"
                        },
                        new
                        {
                            LogID = 34,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 21, 0, 15, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_002T.r4uset"
                        },
                        new
                        {
                            LogID = 35,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 21, 0, 15, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_TH_001E.r4uset"
                        },
                        new
                        {
                            LogID = 36,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 21, 0, 15, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_003T.r4uset"
                        },
                        new
                        {
                            LogID = 37,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 21, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_GU_001E.r4uset"
                        },
                        new
                        {
                            LogID = 38,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_DJ_001B.r4uset"
                        },
                        new
                        {
                            LogID = 39,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_RZ_001T.r4uset"
                        },
                        new
                        {
                            LogID = 40,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_001T.r4uset"
                        },
                        new
                        {
                            LogID = 41,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/gochiusa_bp.r4uset"
                        },
                        new
                        {
                            LogID = 42,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HG_001T.r4uset"
                        },
                        new
                        {
                            LogID = 43,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_RZ_001B.r4uset"
                        },
                        new
                        {
                            LogID = 44,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 13, 47, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_SSSS_001B.r4uset"
                        },
                        new
                        {
                            LogID = 45,
                            Activity = 0,
                            DateAdded = new DateTime(2021, 8, 23, 15, 16, 0, 0, DateTimeKind.Unspecified),
                            IsDone = false,
                            Target = "https://raw.githubusercontent.com/ronelm2000/r4utools/master/Montage.RebirthForYou.Tools.CLI/Sets/set_HP_001B.r4uset"
                        });
                });

            modelBuilder.Entity("Montage.RebirthForYou.Tools.CLI.Entities.R4UCard", b =>
                {
                    b.Property<string>("Serial")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ATK")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Color")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Cost")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DEF")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Effect")
                        .HasColumnType("TEXT");

                    b.Property<string>("Flavor")
                        .HasColumnType("TEXT");

                    b.Property<string>("Images")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Language")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NonFoilSerial")
                        .HasColumnType("TEXT");

                    b.Property<string>("Rarity")
                        .HasColumnType("TEXT");

                    b.Property<string>("Remarks")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SetReleaseID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Serial")
                        .HasName("Serial");

                    b.HasIndex("NonFoilSerial");

                    b.HasIndex("SetReleaseID");

                    b.ToTable("R4UCards");
                });

            modelBuilder.Entity("R4UReleaseSet", b =>
                {
                    b.Property<int>("ReleaseID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ReleaseID");

                    b.ToTable("R4UReleaseSets");
                });

            modelBuilder.Entity("Montage.RebirthForYou.Tools.CLI.Entities.R4UCard", b =>
                {
                    b.HasOne("Montage.RebirthForYou.Tools.CLI.Entities.R4UCard", "NonFoil")
                        .WithMany("Alternates")
                        .HasForeignKey("NonFoilSerial")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("R4UReleaseSet", "Set")
                        .WithMany("Cards")
                        .HasForeignKey("SetReleaseID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("Montage.RebirthForYou.Tools.CLI.Entities.MultiLanguageString", "Name", b1 =>
                        {
                            b1.Property<string>("R4UCardSerial")
                                .HasColumnType("TEXT");

                            b1.Property<string>("EN")
                                .HasColumnType("TEXT");

                            b1.Property<string>("JP")
                                .HasColumnType("TEXT");

                            b1.HasKey("R4UCardSerial");

                            b1.ToTable("R4UCards");

                            b1.WithOwner()
                                .HasForeignKey("R4UCardSerial");
                        });

                    b.OwnsMany("Montage.RebirthForYou.Tools.CLI.Entities.MultiLanguageString", "Traits", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("INTEGER")
                                .HasAnnotation("Sqlite:Autoincrement", true);

                            b1.Property<string>("EN")
                                .HasColumnType("TEXT");

                            b1.Property<string>("JP")
                                .HasColumnType("TEXT");

                            b1.Property<string>("R4UCardSerial")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("Id");

                            b1.HasIndex("R4UCardSerial");

                            b1.ToTable("R4UCards_Traits");

                            b1.WithOwner()
                                .HasForeignKey("R4UCardSerial");
                        });

                    b.Navigation("Name");

                    b.Navigation("NonFoil");

                    b.Navigation("Set");

                    b.Navigation("Traits");
                });

            modelBuilder.Entity("Montage.RebirthForYou.Tools.CLI.Entities.R4UCard", b =>
                {
                    b.Navigation("Alternates");
                });

            modelBuilder.Entity("R4UReleaseSet", b =>
                {
                    b.Navigation("Cards");
                });
#pragma warning restore 612, 618
        }
    }
}
