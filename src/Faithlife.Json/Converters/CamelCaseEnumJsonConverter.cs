using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Uses camel case when rendering enumerated types.
	/// </summary>
	/// <remarks>Same as new StringEnumConverter { CamelCaseText = true }, but can be used
	/// with JsonConverterAttribute.</remarks>
	public class CamelCaseEnumJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return s_stringEnumConverter.CanConvert(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			try
			{
				return s_stringEnumConverter.ReadJson(reader, objectType, existingValue, serializer);
			}
			catch (ArgumentException x)
			{
				throw new JsonSerializationException("Error deserializing enumerated type.", x);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			s_stringEnumConverter.WriteJson(writer, value, serializer);
		}

		static readonly StringEnumConverter s_stringEnumConverter = new StringEnumConverter { CamelCaseText = true };
	}
}
