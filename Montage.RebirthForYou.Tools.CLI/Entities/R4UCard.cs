﻿using Flurl.Http;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class R4UCard : IExactCloneable<R4UCard>
    {
        private static ILogger Log;
        /*
        private static string[] foilRarities = new[] { "SR", "SSR", "RRR", "SPM", "SPa", "SPb", "SP", "SSP", "SEC", "XR", "BDR" };
        private static string[] englishEditedPrefixes = new[] { "EN-", "S25", "W30" };
        private static string[] englishOriginalPrefixes = new[] { "Wx", "SX", "BSF", "BCS" };
        */

        public static IEqualityComparer<R4UCard> SerialComparer { get; internal set; } = new R4USerialComparerImpl();

        public string Serial { get; set; }

        public MultiLanguageString Name { get; set; }
        public List<MultiLanguageString> Traits { get; set; }
        public CardType Type { get; set; }
        public CardColor Color { get; set; }
        public CardSide Side { get; set; }
        public string Rarity { get; set; }

        public int? Level { get; set; }
        public int? Cost { get; set; }
        public int? Soul { get; set; }
        public int? Power { get; set; }
        public Trigger[] Triggers { get; set; }
        public string Flavor { get; set; }
        public string[] Effect { get; set; }
        public List<Uri> Images { get; set; } = new List<Uri>();
        public string Remarks { get; set; }
        
        /// <summary>
        /// File Path Relative Link into a cached image. This property is usually assigned exactly once by
        /// <see cref="IExportedDeckInspector">Deck Inspectors</see>
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public string CachedImagePath { get; set; }

        //public readonly R4UCard Empty = new R4UCard();

        public R4UCard()
        {
            Log ??= Serilog.Log.ForContext<R4UCard>();
        }

        /// <summary>
        /// Gets the Full Release ID
        /// </summary>
        public string ReleaseID => ParseRID(Serial); // Serial.AsSpan().Slice(s => s.IndexOf('/') + 1); s => s.IndexOf('-')).ToString();
        public CardLanguage Language => CardLanguage.Japanese;

        public R4UCard Clone()
        {
            R4UCard newCard = (R4UCard) this.MemberwiseClone();
            newCard.Name = this.Name.Clone();
            newCard.Traits = this.Traits.Select(s => s.Clone()).ToList();
            return newCard;
        }

        public async Task<System.IO.Stream> GetImageStreamAsync()
        {
            int retry = 0;
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
            do try
                {
                    return await Images.Last().WithImageHeaders().GetStreamAsync();
                }
                catch (Exception e) {
                    if (retry++ > 9) throw e;
                } 
            while (true);
        }
        
        private static bool IsExceptionalSerial(string serial)
        {
            var (NeoStandardCode, ReleaseID, SetID) = ParseSerial(serial);
            if (ReleaseID == "W02" && SetID.StartsWith("E")) return true; // https://heartofthecards.com/code/cardlist.html?pagetype=ws&cardset=wslbexeb is an exceptional serial.
            else return false;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a new serial which is the non-foil version.
        /// </summary>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static string RemoveFoil(string serial)
        {
            var parsedSerial = ParseSerial(serial);
            var regex = new Regex(@"([A-Z]*)([0-9]+)([a-z]*)([a-zA-Z]*)");
            if (regex.Match(parsedSerial.SetID) is Match m) parsedSerial.SetID = $"{m.Groups[1]}{m.Groups[2]}{m.Groups[3]}";
            return parsedSerial.AsString();
        }

        public static SerialTuple ParseSerial(string serial)
        {
            SerialTuple res = new SerialTuple();
            res.NeoStandardCode = serial.Substring(0, serial.IndexOf('/'));
            var slice = serial.AsSpan().Slice(serial.IndexOf('/'));
            res.ReleaseID = ParseRID(serial);
            slice = slice.Slice(res.ReleaseID.Length + 2);
            res.SetID = slice.ToString();
            //res.
            return res;
        }

        private static string ParseRID(string serial)
        {
            var span = serial.AsSpan().Slice(s => s.IndexOf('/') + 1);
            var endAdjustment = (span.StartsWith("EN")) ? 3 : 0;
            return span.Slice(0, span.Slice(endAdjustment).IndexOf('-') + endAdjustment).ToString();
        }

        public static string GetSerial(string subset, string side, string lang, string releaseID, string setID)
        {
            string fullSetID = subset;
            if (TryGetExceptionalSetFormat(lang, side + releaseID, out var formatter))
            {
                return formatter((subset, side, lang, releaseID, setID));
            }
            else if (TryGetExceptionalCardFormat(lang, releaseID, setID, out var formatter2))
            {
                return formatter2((subset, side, lang, releaseID, setID));
            }
            else if (lang == "EN" && !setID.Contains("E") && !releaseID.StartsWith("X"))
            {
                return $"{subset}/EN-{side}{releaseID}-{setID}"; // This is a DX set serial adjustment.
            }
            else
            {
                return $"{subset}/{side}{releaseID}-{setID}";
            }
        }

        private static bool TryGetExceptionalSetFormat(string lang, string fullReleaseID, out Func<(string subset, string side, string lang, string releaseID, string setID), string> formatter)
        {
            formatter = (lang, fullReleaseID) switch {
                ("EN", "S04") => (tuple) => $"{tuple.subset}/EN-{tuple.side}{tuple.releaseID}-{tuple.setID}",
                _ => null
            };
            return formatter != null;
        }

        private static bool TryGetExceptionalCardFormat(string lang, string releaseID, string setID, out Func<(string subset, string side, string lang, string releaseID, string setID), string> formatter)
        {
            formatter = (lang, releaseID, setID) switch
            {
                ("EN", "X01", "X02") => (tuple) => "BNJ/BCS2019-02",
                var tuple when tuple.lang == "EN" && tuple.setID.Contains("-") => (tuple) => $"{tuple.subset}/{tuple.setID}",
                _ => null
            };
            return formatter != null;
        }

        public string TypeToString(){
            string res = "";
            switch(this.Type){
                case CardType.Character:
                    res = "CH";
                    break; 
                case CardType.Event:
                    res = "EV";
                    break; 
                case CardType.Climax:
                    res = "CX";
                    break; 
            }
            return res;
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
            CardType.Event => "EV",
            CardType.Climax => "CX",
            var str => throw new Exception($"Cannot parse {typeof(CardType).Name} from {str}")
        };
    }

    public enum EnglishSetType
    {
        JapaneseImport,
        EnglishEdition,
        EnglishOriginal
    }

    public enum CardType
    {
        Character,
        Event,
        Climax
    }


    public enum CardColor
    {
        Yellow,
        Green,
        Red,
        Blue,
        Purple
    }

    public enum Trigger
    {
        Soul,
        Shot,
        Bounce,
        Choice,
        GoldBar,
        Bag,
        Door,
        Standby,
        Book,
        Gate
    }

    public enum CardSide
    {
        Weiss,
        Schwarz,
        Both
    }

    public enum CardLanguage
    {
        English,
        Japanese
    }
}
