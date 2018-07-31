using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace Faithlife.Json.ContractResolvers
{
	public sealed class SnakeCasePropertyNamesContractResolver : DefaultContractResolver
	{
		protected override string ResolvePropertyName(string propertyName)
		{
			return s_snakeCaseRegex.Replace(propertyName,
				match => (match.Groups[1].Success ? match.Groups[1] + "_" : "") + match.Groups[2].ToString().ToLowerInvariant());
		}

		private static readonly Regex s_snakeCaseRegex = new Regex(@"([a-z])?([A-Z]+)");
	}
}
