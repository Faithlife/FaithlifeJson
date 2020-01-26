using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Parses and renders a date (no time) in ISO format.
	/// </summary>
	public class IsoDateOnlyJsonConverter : JsonConverterBase<DateTime>
	{
		protected override void WriteCore(JsonWriter writer, DateTime value, JsonSerializer serializer) =>
			writer.WriteValue(value.ToString(c_dateFormat, CultureInfo.InvariantCulture));

		protected override DateTime ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException("Expected string for DateTime; got " + reader.TokenType);

			var text = reader.Value.ToString();
			try
			{
				return DateTime.ParseExact(text, c_dateFormat, CultureInfo.InvariantCulture);
			}
			catch (FormatException x)
			{
				throw new JsonSerializationException("Error deserializing date.", x);
			}
		}

		const string c_dateFormat = "yyyy'-'MM'-'dd";
	}
}
