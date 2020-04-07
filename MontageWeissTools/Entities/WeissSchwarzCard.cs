﻿using Montage.Weiss.Tools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Montage.Weiss.Tools.Entities
{
    public class WeissSchwarzCard : IExactCloneable<WeissSchwarzCard>
    {
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

        //public readonly WeissSchwarzCard Empty = new WeissSchwarzCard();

        /// <summary>
        /// Gets the Full Release ID
        /// </summary>
        public string ReleaseID => ParseRID(); // Serial.AsSpan().Slice(s => s.IndexOf('/') + 1); s => s.IndexOf('-')).ToString();

        public CardLanguage Language => TranslateToLanguage();


        public WeissSchwarzCard Clone()
        {
            WeissSchwarzCard newCard = (WeissSchwarzCard) this.MemberwiseClone();
            newCard.Name = this.Name.Clone();
            newCard.Traits = this.Traits.Select(s => s.Clone()).ToList();
            return newCard;
        }
        
        private CardLanguage TranslateToLanguage()
        {
            var serial = Serial;
            if (serial.Contains("-E")) return CardLanguage.English;
            else if (serial.Contains("-PE")) return CardLanguage.English;
            else if (serial.Contains("-TE")) return CardLanguage.English;
            else if (serial.Contains("-WX")) return CardLanguage.English;
            else if (serial.Contains("-SX")) return CardLanguage.English;
            else if (serial.Contains("/EN-")) return CardLanguage.English;
            else return CardLanguage.Japanese;
        }
        private string ParseRID()
        {
            var span = Serial.AsSpan().Slice(s => s.IndexOf('/') + 1);
            var endAdjustment = (span.StartsWith("EN")) ? 3 : 0;
            return span.Slice(0, span.Slice(endAdjustment).IndexOf('-') + endAdjustment).ToString();
        }

        public static string GetSerial(string subset, string side, string lang, string releaseID, string setID)
        {
            string fullSetID = subset;
            if (lang == "EN" && !setID.Contains("E"))
            {
                // This is a DX set; make serial adjustments.
                fullSetID += "/EN-" + side;
            }
            else
            {
                // Proceed as normal
                fullSetID += "/" + side;
            }
            fullSetID += releaseID;
            return fullSetID + "-" + setID;
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