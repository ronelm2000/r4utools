﻿using Montage.RebirthForYou.Tools.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
    public class MultiLanguageString : IExactCloneable<MultiLanguageString>
    {
        private IDictionary<string, string> resources = new Dictionary<String,String>();

        public string this[string languageIndex]
        {
            get { return resources[languageIndex]; }
            set { resources[languageIndex] = value; }
        }

//        public int Version { get; set; } = 0;

        // Defaults so that JSON can view it.

        public string EN
        {
            get { return resources["en"]; }
            set { resources["en"] = value; }
        }
        public string JP
        {
            get { return resources["jp"]; }
            set { resources["jp"] = value; }
        }

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
        internal string AsNonEmptyString()
        {
            StringBuilder sb = new StringBuilder();
            if (EN != null)
                sb.Append(EN);
            else
                sb.Append(JP);

            // if (JP != null)
            //     sb.Append($" ({JP})");
            return sb.ToString();
        }

        /*
        internal MultiLanguageString ResolveConflicts(MultiLanguageString oldString)
        {
            if (oldString.Version >= Version)
            {
                resources = oldString.resources;
                Version = oldString.Version;
            }
            return this;
        }
        */
    }
}
