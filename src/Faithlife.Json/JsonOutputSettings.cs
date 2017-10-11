using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Faithlife.Json
{
	/// <summary>
	/// Settings for JsonUtility.ToJson.
	/// </summary>
	public class JsonOutputSettings
	{
		/// <summary>
		/// Gets or sets the converters.
		/// </summary>
		/// <value>The converters.</value>
		public IReadOnlyList<JsonConverter> Converters { get; set; }

		/// <summary>
		/// True if the JSON should be indented.
		/// </summary>
		public bool IsIndented { get; set; }

		/// <summary>
		/// Gets or sets the contract resolver.
		/// </summary>
		/// <value>The contract resolver.</value>
		public IContractResolver ContractResolver { get; set; }
	}
}
