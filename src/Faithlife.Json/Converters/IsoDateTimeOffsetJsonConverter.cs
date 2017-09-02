using System;
using System.Globalization;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Parses or renders a DateTimeOffset.
	/// </summary>
	/// <remarks>Uses DateTimeOffsetUtility.Iso8601Format</remarks>
	public class IsoDateTimeOffsetJsonConverter : JsonConverterBase<DateTimeOffset>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IsoDateTimeOffsetJsonConverter"/> class.
		/// </summary>
		public IsoDateTimeOffsetJsonConverter()
		{
		}

		/// <summary>
		/// Overrides WriteCore.
		/// </summary>
		protected override void WriteCore(JsonWriter writer, DateTimeOffset value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString(DateTimeOffsetUtility.Iso8601Format, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Overrides ReadCore.
		/// </summary>
		protected override DateTimeOffset ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			// value was serialized by default JsonConvert
			if (reader.TokenType == JsonToken.Date)
				return new DateTimeOffset((DateTime) reader.Value);

			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException("Expected string for DateTimeOffset; got " + reader.TokenType);

			string text = reader.Value.ToString();

			try
			{
				return DateTimeOffset.ParseExact(text,
					DateTimeOffsetUtility.Iso8601Format, CultureInfo.InvariantCulture,
					DateTimeStyles.None);
			}
			catch (FormatException ex)
			{
				throw new JsonSerializationException("Failed to parse ISO 8601 string '{0}'".FormatInvariant(text), ex);
			}
		}
	}
}
