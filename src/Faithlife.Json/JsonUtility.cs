using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Faithlife.Json.Converters;
using Faithlife.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Faithlife.Json
{
	/// <summary>
	/// Helper methods for working with Json.NET.
	/// </summary>
	public static class JsonUtility
	{
		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The JSON.</returns>
		public static string ToJson(object value)
		{
			return ToJson(value, null);
		}

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON.</returns>
		public static string ToJson(object value, JsonSettings settings)
		{
			using (StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture))
			{
				ToJsonTextWriter(value, settings, stringWriter);
				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// Converts the object to a JSON writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="jsonWriter">The JSON writer to write JSON to.</param>
		public static void ToJsonWriter(object value, JsonWriter jsonWriter)
		{
			ToJsonWriter(value, null, jsonWriter);
		}

		/// <summary>
		/// Converts the object to a JSON writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="jsonWriter">The JSON writer to write JSON to.</param>
		public static void ToJsonWriter(object value, JsonSettings settings, JsonWriter jsonWriter)
		{
			JsonSerializer serializer = JsonSerializer.Create(CreateDefaultJsonSerializerSettings(settings));
			serializer.Serialize(jsonWriter, value);
		}

		/// <summary>
		/// Converts the object to a JSON text writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="textWriter">The text writer to write JSON to.</param>
		public static void ToJsonTextWriter(object value, TextWriter textWriter)
		{
			ToJsonTextWriter(value, null, textWriter);
		}

		/// <summary>
		/// Converts the object to a JSON text writer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="textWriter">The text writer to write JSON to.</param>
		public static void ToJsonTextWriter(object value, JsonSettings settings, TextWriter textWriter)
		{
			Formatting formatting = GetJsonFormatting(settings);
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(textWriter) { Formatting = formatting, CloseOutput = false })
				ToJsonWriter(value, settings, jsonTextWriter);
		}

		/// <summary>
		/// Converts the object to a JSON stream.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="outputStream">The stream to write JSON to, using UTF-8 encoding.</param>
		public static void ToJsonStream(object value, Stream outputStream)
		{
			ToJsonStream(value, null, outputStream);
		}

		/// <summary>
		/// Converts the object to a JSON stream.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="outputStream">The stream to write JSON to, using UTF-8 encoding.</param>
		public static void ToJsonStream(object value, JsonSettings settings, Stream outputStream)
		{
			// don't dispose the StreamWriter to avoid closing the stream
			StreamWriter textWriter = new StreamWriter(outputStream);
			ToJsonTextWriter(value, settings, textWriter);
			textWriter.Flush();
		}

		/// <summary>
		/// Converts the object to compressed JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The compressed JSON (as compressed by StringUtility).</returns>
		public static byte[] ToCompressedJson(object value)
		{
			return ToCompressedJson(value, null);
		}

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON.</returns>
		public static byte[] ToCompressedJson(object value, JsonSettings settings)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (TextWriter textWriter = StringUtility.CreateCompressingTextWriter(stream, Ownership.None))
					ToJsonTextWriter(value, textWriter);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Returns the number of bytes used by the JSON of an object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The number of bytes used by the JSON of an object.</returns>
		public static int ToJsonByteCount(object value)
		{
			return ToJsonByteCount(value, null);
		}

		/// <summary>
		/// Returns the number of bytes used by the JSON of an object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The number of bytes used by the JSON of an object.</returns>
		public static int ToJsonByteCount(object value, JsonSettings settings)
		{
			using (ZeroStream zeroStream = new ZeroStream())
			{
				ToJsonStream(value, settings, zeroStream);
				return (int) zeroStream.Length;
			}
		}

		/// <summary>
		/// Converts the object to a JToken.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The JToken.</returns>
		public static JToken ToJToken(object value)
		{
			return ToJToken(value, null);
		}

		/// <summary>
		/// Converts the object to a JToken.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The JToken.</returns>
		public static JToken ToJToken(object value, JsonSettings settings)
		{
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				ToJsonWriter(value, settings, jTokenWriter);
				return jTokenWriter.Token;
			}
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJson<T>(string json)
		{
			return FromJson<T>(json, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="json">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJson(string json, Type type)
		{
			return FromJson(json, type, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJson<T>(string json, JsonSettings settings)
		{
			return (T) FromJson(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="json">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJson(string json, Type type, JsonSettings settings)
		{
			using (StringReader stringReader = new StringReader(json))
				return FromJsonTextReader(stringReader, type, settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJsonTextReader<T>(TextReader textReader)
		{
			return FromJsonTextReader<T>(textReader, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJsonTextReader(TextReader textReader, Type type)
		{
			return FromJsonTextReader(textReader, type, null);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromJsonTextReader<T>(TextReader textReader, JsonSettings settings)
		{
			return (T) FromJsonTextReader(textReader, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from JSON.
		/// </summary>
		/// <param name="textReader">The JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromJsonTextReader(TextReader textReader, Type type, JsonSettings settings)
		{
			using (JsonReader reader = new JsonTextReader(textReader))
				return Deserialize(settings, reader, type);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The compressed JSON.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromCompressedJson<T>(byte[] json)
		{
			return FromCompressedJson<T>(json, null);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromCompressedJson(byte[] json, Type type)
		{
			return FromCompressedJson(json, type, null);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static T FromCompressedJson<T>(byte[] json, JsonSettings settings)
		{
			return (T) FromCompressedJson(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from compressed JSON.
		/// </summary>
		/// <param name="json">The compressed JSON.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		/// <exception cref="JsonReaderException">The text is not valid JSON.</exception>
		/// <exception cref="JsonSerializationException">The JSON cannot be deserialized into the specified type.</exception>
		public static object FromCompressedJson(byte[] json, Type type, JsonSettings settings)
		{
			using (MemoryStream stream = new MemoryStream(json, false))
			using (TextReader textReader = StringUtility.CreateDecompressingTextReader(stream, Ownership.None))
				return FromJsonTextReader(textReader, type, settings);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JToken.</param>
		/// <returns>The object.</returns>
		public static T FromJToken<T>(JToken json)
		{
			return FromJToken<T>(json, null);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <param name="json">The JToken.</param>
		/// <param name="type">The type.</param>
		/// <returns>The object.</returns>
		public static object FromJToken(JToken json, Type type)
		{
			return FromJToken(json, type, null);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <param name="json">The JToken.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		public static T FromJToken<T>(JToken json, JsonSettings settings)
		{
			return (T) FromJToken(json, typeof(T), settings);
		}

		/// <summary>
		/// Creates an object from a JToken.
		/// </summary>
		/// <param name="json">The JToken.</param>
		/// <param name="type">The type.</param>
		/// <param name="settings">The settings.</param>
		/// <returns>The object.</returns>
		public static object FromJToken(JToken json, Type type, JsonSettings settings)
		{
			if (json.IsNull())
				return json;

			using (JsonReader reader = new JTokenReader(json))
				return Deserialize(settings, reader, type);
		}

		/// <summary>
		/// Creates default serialization settings.
		/// </summary>
		/// <returns>The serialization settings used by ToJson and FromJson.</returns>
		public static JsonSerializerSettings CreateDefaultJsonSerializerSettings()
		{
			return CreateDefaultJsonSerializerSettings(null);
		}

		/// <summary>
		/// Creates default serialization settings.
		/// </summary>
		/// <returns>The serialization settings used by ToJson.</returns>
		public static JsonSerializerSettings CreateDefaultJsonSerializerSettings(JsonSettings settings)
		{
			settings = settings ?? new JsonSettings();

			JsonSerializerSettings serializerSettings =
				new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					DateParseHandling = DateParseHandling.None,
					NullValueHandling = settings.IncludesNullValues ? NullValueHandling.Include : NullValueHandling.Ignore,
					MissingMemberHandling = settings.RejectsExtraProperties ? MissingMemberHandling.Error : MissingMemberHandling.Ignore,
					MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
					CheckAdditionalContent = true,
				};

			if (settings.Converters != null)
				serializerSettings.Converters.AddRange(settings.Converters);

			serializerSettings.Converters.AddRange(s_defaultConverters);

			return serializerSettings;
		}

		/// <summary>
		/// Gets the JSON formatting specified by the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The JSON formatting.</returns>
		public static Formatting GetJsonFormatting(JsonSettings settings)
		{
			return settings != null && settings.IsIndented ? Formatting.Indented : Formatting.None;
		}

		private static object Deserialize(JsonSettings settings, JsonReader reader, Type type)
		{
			JsonSerializer serializer = JsonSerializer.Create(CreateDefaultJsonSerializerSettings(settings));
			object value = serializer.Deserialize(reader, type);
			if (value == null && type == typeof(JToken))
				value = new JValue((object) null);
			return value;
		}

		private static readonly IReadOnlyList<JsonConverter> s_defaultConverters =
			new JsonConverter[]
			{
				new CamelCaseEnumJsonConverter(),
				new IsoDateTimeUtcJsonConverter(),
				new IsoDateTimeOffsetJsonConverter(),
				new DictionaryKeysAreNotPropertyNamesJsonConverter(), // NOTE: must be after any other dictionary converters
			};
	}
}
