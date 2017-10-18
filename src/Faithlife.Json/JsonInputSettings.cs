using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Faithlife.Json
{
	/// <summary>
	/// Settings for JsonUtility.FromJson.
	/// </summary>
	public class JsonInputSettings
	{
		/// <summary>
		/// Gets or sets the converters.
		/// </summary>
		/// <value>The converters.</value>
		public IReadOnlyList<JsonConverter> Converters { get; set; }

		/// <summary>
		/// True if JSON properties without corresponding .NET object properties should be ignored.
		/// </summary>
		/// <value>True if JSON properties without corresponding .NET object properties should be ignored.</value>
		public bool IgnoresExtraProperties { get; set; }

		/// <summary>
		/// True if "null" JSON properties should be read.
		/// </summary>
		/// <value>True if "null" JSON properties should be read.</value>
		public bool IncludesNullValues { get; set; }

		/// <summary>
		/// Gets or sets the contract resolver.
		/// </summary>
		/// <value>The contract resolver.</value>
		public IContractResolver ContractResolver { get; set; }
	}
}
