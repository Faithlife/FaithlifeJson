using System;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Uses integers when rendering enumerated types.
	/// </summary>
	public class EnumAsIntegerJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (Nullable.GetUnderlyingType(objectType) ?? objectType).IsEnum();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type underlyingType = Nullable.GetUnderlyingType(objectType);

			if (reader.TokenType == JsonToken.Null)
			{
				if (underlyingType == null)
					throw new JsonSerializationException("Cannot convert null value to non-nullable enum.");

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
				return Enum.Parse(underlyingType ?? objectType, readerValue, true);
			}
			catch (ArgumentException x)
			{
				throw new JsonSerializationException("Error deserializing enumerated type.", x);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(Convert.ToInt64(value));
		}
	}
}
