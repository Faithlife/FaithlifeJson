using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Faithlife.Utility;
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
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		public JsonOutputSettings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonOutputSettings(params JsonConverter[] converters)
			: this(null, converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonOutputSettings(IEnumerable<JsonConverter> converters)
			: this(null, converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		public JsonOutputSettings(JsonOutputSettings settings)
			: this(settings, (IEnumerable<JsonConverter>) null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonOutputSettings(JsonOutputSettings settings, params JsonConverter[] converters)
			: this(settings, (IEnumerable<JsonConverter>) converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonOutputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonOutputSettings(JsonOutputSettings settings, IEnumerable<JsonConverter> converters)
		{
			if (settings != null)
			{
				if (settings.Converters != null || converters != null)
				{
					if (settings.Converters == null)
						Converters = converters.ToList().AsReadOnly();
					else if (converters == null)
						Converters = settings.Converters;
					else
						Converters = settings.Converters.Concat(converters).ToList().AsReadOnly();
				}

				IsIndented = settings.IsIndented;
			}
			else if (converters != null)
			{
				Converters = converters.ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Gets or sets the converters.
		/// </summary>
		/// <value>The converters.</value>
		public ReadOnlyCollection<JsonConverter> Converters { get; set; }

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
