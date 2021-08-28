using CommandLine;
using Fluent.IO;
using Flurl.Http;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.RebirthForYou.Tools.CLI.API;
using Montage.RebirthForYou.Tools.CLI.Entities;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.CLI
{
    [Verb("cache", HelpText = "Downloads all related images and updates it into a file; also edits the image metadata to show proper attribution.")]
    public class CacheVerb : IVerbCommand
    {
        private static readonly string _IMAGE_CACHE_PATH = "./Images/";
        private ILogger Log = Serilog.Log.ForContext<CacheVerb>();

        [Value(0, HelpText = "Indicates either Release ID or a full Serial ID.")]
        public string ReleaseIDorFullSerialID { get; set; }

        [Value(1, HelpText = "Indicates the Language. This is only applicable if you indicated the Release ID.", Default = "")]
        public string Language { get; set; } = "";

        public async Task Run(IContainer ioc)
        {
            Log.Information("Starting.");
            var language = InterpretLanguage(Language);
            IAsyncEnumerable<R4UCard> list = null;

            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                
                if (language == null)
                {
                    var query = from card in db.R4UCards.AsQueryable()
                                where card.Serial.ToLower() == ReleaseIDorFullSerialID.ToLower()
                                select card;
                    list = query.ToAsyncEnumerable().Take(1);
                } 
                else
                {
                    var setResult = from set in db.R4UReleaseSets.AsQueryable()
                                    where set.ReleaseCode.ToLower() == ReleaseIDorFullSerialID.ToLower()
                                    select set;
                    list = (await setResult.FirstAsync()).Cards.ToAsyncEnumerable();
                }

                await foreach (var card in list.Where(c => !c.IsCached))
                    await AddCachedImageAsync(card);

                Log.Information("Done.");
                Log.Information("PS: Please refrain from executing this command continuously as this may cause your IP address to get tagged as a DDoS bot.");
                Log.Information("    Only cache the images you may need.");
                Log.Information("    -ronelm2000");

            }
        }

        public async Task AddCachedImageAsync(R4UCard card)
        {
            try
            {
                var imgURL = card.Images.Last();
                Log.Information("Caching [{serial}]: {imgURL}", card.Serial, imgURL);
                //using (System.IO.Stream netStream = await imgURL.WithImageHeaders().GetStreamAsync()) // card.GetImageStreamAsync())
                await using (var bytes = await imgURL
                    .WithImageHeaders()
                    .WithRateLimiter()
                    .GetStreamAsync()
                    )
                using (Image img = Image.Load(bytes))
                {
                    var imageDirectoryPath = Path.Get(_IMAGE_CACHE_PATH);
                    if (!imageDirectoryPath.Exists) imageDirectoryPath.CreateDirectory();
                    if (img.Width > img.Height)
                    {
                        Log.Debug("Rotating Image as it's Cached...");
                        img.Mutate(ctx => ctx.Rotate(RotateMode.Rotate90));
                    }
                    img.Metadata.ExifProfile ??= new ExifProfile();
                    img.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Copyright, card.Images.Last().Authority);
                    var savePath = Path.Get(_IMAGE_CACHE_PATH).Combine($"{card.Serial.Replace('-', '_').AsFileNameFriendly()}.jpg");
                    await using (var stream = savePath.OpenStream(System.IO.FileMode.OpenOrCreate))
                        await img.SaveAsJpegAsync(stream);
                }
            } catch (InvalidOperationException e) when (e.Message == "Sequence contains no elements")
            {
                Log.Warning("Cannot be cached as no image URLs were found: {serial}", card.Serial);
            }
        }

        private CardLanguage? InterpretLanguage(string language)
        {
            return language switch
            {
                var l when (l.ToLower() == "en") => CardLanguage.English,
                var l when (l.ToLower() == "jp") => CardLanguage.Japanese,
                _ => null // meaning any
            };
        }
    }
}
