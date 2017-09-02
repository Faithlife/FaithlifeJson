#pragma warning disable 1591

using System;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Parses and renders a GUID as a no-dash lowercase string.
	/// </summary>
	public class GuidLowerNoDashJsonConverter : JsonConverterBase<Guid>
	{
		protected override void WriteCore(JsonWriter writer, Guid value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToLowerNoDashString());
		}

		protected override Guid ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException("Expected string for Guid; got " + reader.TokenType);

			try
			{
				return GuidUtility.FromLowerNoDashString((string) reader.Value);
			}
			catch (FormatException x)
			{
				throw new JsonSerializationException("Error deserializing GUID.", x);
			}
		}
	}
}
