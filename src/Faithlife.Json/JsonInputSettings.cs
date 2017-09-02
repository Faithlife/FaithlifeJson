using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Faithlife.Utility;
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
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		public JsonInputSettings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonInputSettings(params JsonConverter[] converters)
			: this(null, converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonInputSettings(IEnumerable<JsonConverter> converters)
			: this(null, converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		public JsonInputSettings(JsonInputSettings settings)
			: this(settings, (IEnumerable<JsonConverter>) null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonInputSettings(JsonInputSettings settings, params JsonConverter[] converters)
			: this(settings, (IEnumerable<JsonConverter>) converters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonInputSettings"/> class.
		/// </summary>
		/// <param name="settings">The settings to copy from, if any.</param>
		/// <param name="converters">The converters to use, if any.</param>
		public JsonInputSettings(JsonInputSettings settings, IEnumerable<JsonConverter> converters)
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

				IgnoresExtraProperties = settings.IgnoresExtraProperties;
				IncludesNullValues = settings.IncludesNullValues;
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
