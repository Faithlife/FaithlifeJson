using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Writes ulong values as strings. This avoids a JsonReaderException for values greater than long.MaxValue.
	/// </summary>
	public sealed class UInt64JsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (Nullable.GetUnderlyingType(objectType) ?? objectType) == typeof(ulong);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				if (Nullable.GetUnderlyingType(objectType) == null)
					throw new JsonSerializationException("Cannot convert null value to non-nullable ulong.");

				return null;
			}

			string readerValue;
			if (reader.TokenType == JsonToken.Integer)
				readerValue = reader.Value.ToString();
			else if (reader.TokenType == JsonToken.String)
				readerValue = (string) reader.Value;
			else
				throw new JsonSerializationException("Expected integer or string; got " + reader.TokenType);

			try
			{
				return ulong.Parse(readerValue, CultureInfo.InvariantCulture);
			}
			catch (ArgumentException x)
			{
				throw new JsonSerializationException("Error deserializing ulong.", x);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value != null)
				writer.WriteValue(((ulong) value).ToString(CultureInfo.InvariantCulture));
			else
				writer.WriteValue(value);
		}
	}
}
