using System;
using System.Globalization;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Parses or renders a DateTime of DateTimeKind.Utc.
	/// </summary>
	/// <remarks>Uses DateTimeUtility.Iso8601Format</remarks>
	public class IsoDateTimeUtcJsonConverter : JsonConverterBase<DateTime>
	{
		protected override void WriteCore(JsonWriter writer, DateTime value, JsonSerializer serializer)
		{
			if (value.Kind != DateTimeKind.Utc)
				throw new InvalidOperationException("IsoDateTimeUtcJsonConverter only supports DateTimeKind.Utc but value at path '{0}' is DateTimeKind.{1}.".FormatInvariant(writer.Path, value.Kind));

			writer.WriteValue(value.ToString(DateTimeUtility.Iso8601Format, CultureInfo.InvariantCulture));
		}

		protected override DateTime ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException("Expected string for DateTime; got " + reader.TokenType);

			var text = reader.Value.ToString();

			try
			{
				return DateTime.ParseExact(text,
					DateTimeUtility.Iso8601Format, CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
			}
			catch (FormatException x)
			{
				throw new JsonSerializationException("Error deserializing date.", x);
			}
		}
	}
}
