using System.Collections.Generic;
using Newtonsoft.Json;

namespace Faithlife.Json
{
	/// <summary>
	/// Settings for reading/writing JSON via <see cref="JsonUtility" />.
	/// </summary>
	public class JsonSettings
	{
		/// <summary>
		/// Additional converters to use when reading/writing JSON.
		/// </summary>
		public IReadOnlyList<JsonConverter> Converters { get; set; }

		/// <summary>
		/// True if JSON properties without corresponding object properties should throw an exception when reading JSON.
		/// </summary>
		/// <remarks>The default is <c>false</c>, which ignores any extra JSON properties.</remarks>
		public bool RejectsExtraProperties { get; set; }

		/// <summary>
		/// True if <c>null</c> JSON properties should be read and/or written.
		/// </summary>
		/// <remarks>The default is <c>false</c>, which neither reads nor writes <c>null</c> JSON properties.</remarks>
		public bool IncludesNullValues { get; set; }

		/// <summary>
		/// True if JSON output should be indented.
		/// </summary>
		/// <remarks>The default is <c>false</c>, which generates minimal, compact JSON.</remarks>
		public bool IsIndented { get; set; }
	}
}
