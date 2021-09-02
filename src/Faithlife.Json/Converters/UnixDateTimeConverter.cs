using System;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Handles conversion an integer representing a unix UTC timestamp and a UTC DateTime.
	/// </summary>
	public class UnixDateTimeConverter : JsonConverterBase<DateTime>
	{
		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		protected override DateTime ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Integer)
				throw new JsonSerializationException("Expected integer for DateTime; got " + reader.TokenType);

			return DateTimeUtility.FromUnixTimestamp((long) reader.Value);
		}

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		protected override void WriteCore(JsonWriter writer, DateTime value, JsonSerializer serializer)
		{
			var ticks = DateTimeUtility.ToUnixTimestamp(value);

			if (ticks < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Invalid epoch value.");

			writer.WriteValue(ticks);
		}
	}
}
