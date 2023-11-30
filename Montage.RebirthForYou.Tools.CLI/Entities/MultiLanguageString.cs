using AngleSharp.Common;
using Montage.RebirthForYou.Tools.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class MultiLanguageString : IExactCloneable<MultiLanguageString>
    {
        private readonly IDictionary<string, string> resources = new Dictionary<string, String>();

        public string this[string languageIndex]
        {
            get { return resources[languageIndex]; }
            set { resources[languageIndex] = value; }
        }

//        public int Version { get; set; } = 0;

        // Defaults so that JSON can view it.

        public string EN
        {
            get { return resources.GetOrDefault("en", null); }
            set { resources["en"] = value; }
        }
        public string JP
        {
            get { return resources.GetOrDefault("jp", null); }
            set { resources["jp"] = value; }
        }

        [JsonIgnore]
        public string Default => $"{EN ?? JP}{((JP != null && EN != null) ? $" ({JP})" : "")}"; 

        public MultiLanguageString Clone()
        {
            var newMLS = new MultiLanguageString();
            foreach (var val in resources)
                newMLS.resources.Add(val.Key, val.Value);
            return newMLS;
        }

        /// <summary>
        /// Attempts to resolve this object into a string as much as it can.
        /// </summary>
        /// <returns></returns>
        public string AsNonEmptyString()
        {
            StringBuilder sb = new();
            if (EN != null)
                sb.Append(EN);
            else
                sb.Append(JP);

            // if (JP != null)
            //     sb.Append($" ({JP})");
            return sb.ToString();
        }

        public string AsJPThenEN()
        {
            if (string.IsNullOrWhiteSpace(JP))
                return EN;
            else
                return JP;
        }

        public static implicit operator MultiLanguageString((string EN, string JP) tuple)
            => new MultiLanguageString { EN = tuple.EN, JP = tuple.JP };
    }
}
