using Montage.RebirthForYou.Tools.CLI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Montage.RebirthForYou.Tools.CLI.Entities
{
	public class R4UReleaseSet
	{
		[JsonIgnore]
		public int ReleaseID { get; set; }
		public string ReleaseCode { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		[JsonIgnore]
		public ICollection<R4UCard> Cards { get; set; }

		public R4UReleaseSet()
		{
		}

		[JsonIgnore]
		public string[] TitleCodes => new Regex(@"(?<!\d)([A-Z]+)(?!-)").Matches(ReleaseCode).Select(m => m.Groups[0].Value).ToArray();

		internal static string ByReleaseCode(R4UReleaseSet set) => set.ReleaseCode;
	}
}
