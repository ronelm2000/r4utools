using Flurl.Http;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class R4UCard : IExactCloneable<R4UCard>
    {
        private static ILogger Log;
        private static readonly Uri EmptyURL = new Uri("https://i.imgur.com/31iwAc9.jpg");

        public static IEqualityComparer<R4UCard> SerialComparer { get; internal set; } = new R4USerialComparerImpl();
        private readonly static string _imageCachePath = "./Images/";

        public string Serial { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<R4UCard> Alternates { get; set; }
        [JsonIgnore]
        public R4UCard NonFoil { get; set; }
        public MultiLanguageString Name { get; set; }
        public List<MultiLanguageString> Traits { get; set; }
        public CardType? Type { get; set; }
        public CardColor? Color { get; set; }
        public string Rarity { get; set; }

        public int? Cost { get; set; }
        public int? ATK { get; set; }
        public int? DEF { get; set; }

        public MultiLanguageString Flavor { get; set; }
        public MultiLanguageString[] Effect { get; set; }
        public List<Uri> Images { get; set; } = new List<Uri>();
        public string Remarks { get; set; }
        public CardLanguage? Language { get; set; } = CardLanguage.Japanese;
        public R4UReleaseSet Set { get; set; }

        /// <summary>
        /// File Path Relative Link into a cached image. This property is usually assigned exactly once by
        /// <see cref="IExportedDeckInspector">Deck Inspectors</see>
        /// </summary>
        [JsonIgnore]
        public string CachedImagePath
        {
            get
            {
                try
                {                    
                    var serialImage = Fluent.IO.Path.Get(_imageCachePath)
                            .AllFiles()
                            .Where( p => p.FileNameWithoutExtension.ToLower() == Serial.Replace('-', '_').AsFileNameFriendly().ToLower() )
                            .WhereExtensionIs(".png", ".jpeg", ".jpg", ".jfif")
                            .Append(null)
                            .First();
                    if (serialImage == null) 
                        return null;
                    else
                        return serialImage.FullPath;
                } catch (DirectoryNotFoundException)
                {
                    return null;
                } catch (Exception e)
                {
                    Log.Warning("Error occurred: {message}", e);
                    return null;
                }
            }
        }

        [JsonIgnore]
        public bool IsCached => CachedImagePath != null;

        public R4UCard()
        {
            Log ??= Serilog.Log.ForContext<R4UCard>();
        }

        public R4UCard Clone()
        {
            R4UCard newCard = (R4UCard)this.MemberwiseClone();
            newCard.Name = this.Name.Clone();
            newCard.Traits = this.Traits?.Select(s => s?.Clone()).ToList() ?? new List<MultiLanguageString>();
            newCard.Images = this.Images.ToList();
            return newCard;
        }

        public async Task<System.IO.Stream> GetImageStreamAsync(CookieSession session = default)
        {
            // int retry = 0;
            if (!String.IsNullOrWhiteSpace(CachedImagePath) && !CachedImagePath.Contains(".."))
                try
                {
                    return System.IO.File.OpenRead(CachedImagePath);
                }
                catch (System.IO.FileNotFoundException)
                {
                    Log.Warning("Cannot find cache file: {cacheImagePath}.", CachedImagePath);
                    Log.Warning("Falling back on remote URL.");
                }
                catch (Exception) { }
            var img = Images?.Prepend(EmptyURL).Last();
            Log.Debug("Loading URL: {url}", img.AbsoluteUri);
            var bytes = await Images?.Prepend(EmptyURL).Last().WithImageHeaders().WithCookies(session).GetAsync().WithRetries(10).ReceiveBytes() ?? await EmptyURL.WithImageHeaders().GetBytesAsync();
            return new MemoryStream(bytes);
        }

        public string TypeToString() => this.Type?.AsShortString() ?? "";
        [JsonIgnore]
        public bool IsFoil => NonFoil != null;
        [JsonIgnore]
        public string ReleaseID => Set?.ReleaseCode;

        /// <summary>
        /// Automatically fills up the card with any missing details, usually sourced from a Non Foil version.
        /// </summary>
        public void FillProxy()
        {
            if (NonFoil == null) return;
            Name = NonFoil.Name.Clone();
            Traits = NonFoil.Traits?.Select(s => s?.Clone()).ToList() ?? new List<MultiLanguageString>();
            Type = NonFoil.Type;
            Color = NonFoil.Color;
            Rarity = Rarity ?? NonFoil.Rarity;
            Cost = NonFoil.Cost;
            ATK = NonFoil.ATK;
            DEF = NonFoil.DEF;
            Flavor = NonFoil.Flavor;
            Effect = NonFoil.Effect;
            Language = NonFoil.Language;
            Set = NonFoil.Set;
        }

        public R4UCard AsProxy()
        {
            var result = this.Clone();
            result.Name = null;
            result.Traits = null;
            result.Type = null;
            result.Color = null;
            result.ATK = null;
            result.DEF = null;
            result.Cost = null;
            result.Effect = null;
            result.Language = null;
            result.Set = null;
            return result;
        }
    }


    internal class R4USerialComparerImpl : IEqualityComparer<R4UCard>
    {
        public bool Equals([AllowNull] R4UCard x, [AllowNull] R4UCard y)
        {
            if (x == null) return y == null;
            else return x.Serial == y.Serial;
        }

        public int GetHashCode([DisallowNull] R4UCard obj)
        {
            return obj.Serial.GetHashCode();
        }
    }

    public struct SerialTuple
    {
        public string NeoStandardCode;
        public string ReleaseID;
        public string SetID;

        public void Deconstruct(out string NeoStandardCode, out string ReleaseID, out string SetID)
        {
            NeoStandardCode = this.NeoStandardCode;
            ReleaseID = this.ReleaseID;
            SetID = this.SetID;
        }

        internal string AsString()
        {
            return $"{NeoStandardCode}/{ReleaseID}-{SetID}";
        }
    }

    public static class CardEnumExtensions
    {
        public static T? ToEnum<T>(this ReadOnlySpan<char> stringSpan) where T : struct, System.Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            foreach (var e in values)
                if (stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    return e;
            return null;
            //return values.Where(e => stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase)).First();
        }

        public static T? ToEnum<T>(this string stringSpan) where T : struct, System.Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            foreach (var e in values)
                if (stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    return e;
            return null;
            //return values.Where(e => stringSpan.StartsWith(e.ToString(), StringComparison.CurrentCultureIgnoreCase)).First();
        }

        public static string AsShortString(this CardType cardType) => cardType switch
        {
            CardType.Character => "CH",
            CardType.Rebirth => "RB",
            CardType.Partner => "PTNR",
            var str => throw new Exception($"Cannot parse {typeof(CardType).Name} from {str}")
        };
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardType
    {
        Character,
        Rebirth,
        Partner
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardColor
    {
        Yellow,
        Green,
        Blue,
        Red
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardLanguage
    {
        English,
        Japanese
    }   

}
