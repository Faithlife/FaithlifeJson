using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Faithlife.Utility;
using Newtonsoft.Json;

#pragma warning disable 1591

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// JSON converter for ReadOnlyDictionary.
	/// </summary>
	/// <remarks>When deserializing, the ReadOnlyDictionary wraps a Dictionary.</remarks>
	[Obsolete("Json.NET has built-in support for ReadOnlyDictionary<TKey, TValue>")]
	public class ReadOnlyDictionaryJsonConverter : DictionaryKeysAreNotPropertyNamesJsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType() && objectType.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>);
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments());
			object dictionary = serializer.Deserialize(reader, dictionaryType);

			if (dictionary == null)
				return null;
			return objectType.GetConstructor(new[] { dictionaryType }).Invoke(new[] { dictionary });
		}
	}
}
