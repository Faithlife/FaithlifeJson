using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Faithlife.Json.Converters;
using Faithlife.Utility;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JsonUtilityTests
	{
		[Test]
		public void DefaultSettings()
		{
			Widget widget = new Widget { Title = "title", Kind = new WidgetKind { WeightInGrams = 1.2, ReleaseDate = new DateTime(2010, 9, 8, 7, 6, 5, DateTimeKind.Utc) } };
			const string jsonTo = @"{""title"":""title"",""kind"":{""weightInGrams"":1.2,""releaseDate"":""2010-09-08T07:06:05Z""}}";
			Assert.AreEqual(JsonUtility.ToJson(widget), jsonTo);
			Assert.AreEqual(StringUtility.Decompress(JsonUtility.ToCompressedJson(widget)), jsonTo);
			Assert.AreEqual(JsonUtility.ToJToken(widget).ToString(Formatting.None), jsonTo);
			Assert.AreEqual(JsonUtility.ToJsonByteCount(widget), jsonTo.Length);

			const string jsonFrom = @"{""kind"":{""weightInGrams"":2.4,""releaseDate"":""2010-11-12T13:14:15Z""},""title"":""whatever""}";
			widget = JsonUtility.FromJson<Widget>(jsonFrom);
			Assert.AreEqual(widget.Title, "whatever");
			Assert.AreEqual(widget.Kind.ReleaseDate.Second, 15);

			widget = JsonUtility.FromCompressedJson<Widget>(StringUtility.Compress(jsonFrom));
			Assert.AreEqual(widget.Title, "whatever");
			Assert.AreEqual(widget.Kind.ReleaseDate.Second, 15);

			widget = JsonUtility.FromJToken<Widget>(JsonUtility.FromJson<JToken>(jsonFrom));
			Assert.AreEqual(widget.Title, "whatever");
			Assert.AreEqual(widget.Kind.ReleaseDate.Second, 15);
		}

		[Test]
		public void DeserializingNull()
		{
			Assert.IsNull(JsonUtility.FromJToken<Widget>(null));
			Assert.IsNull(JsonUtility.FromJToken<JToken>(null));
			Assert.IsTrue(JsonUtility.FromJToken<JToken>(new JValue((object) null)).IsNull());
			Assert.IsTrue(JsonUtility.FromJToken<JToken>(new JValue((string) null)).IsNull());
		}

		[Test]
		public void OverrideConverter()
		{
			Widget widget = new Widget { Title = "title", Kind = new WidgetKind { WeightInGrams = 1.2, ReleaseDate = new DateTime(2010, 9, 8, 7, 6, 5, DateTimeKind.Utc) } };
			Assert.AreEqual(JsonUtility.ToJson(widget, new JsonOutputSettings { Converters = new[] { new IsoDateOnlyJsonConverter() } }),
				@"{""title"":""title"",""kind"":{""weightInGrams"":1.2,""releaseDate"":""2010-09-08""}}");

			widget = JsonUtility.FromJson<Widget>(
				@"{""kind"":{""weightInGrams"":2.4,""releaseDate"":""2010-11-12""},""title"":""whatever""}", new JsonInputSettings { Converters = new[] { new IsoDateOnlyJsonConverter() } });
			Assert.AreEqual(widget.Title, "whatever");
			Assert.AreEqual(widget.Kind.ReleaseDate.Day, 12);
		}

		[Test]
		public void DictionaryKeysIgnoreCamelCase()
		{
			var dictionary = new Dictionary<string, string> { { "Foo", "Bar" } };
			Assert.AreEqual(JsonUtility.ToJson(dictionary), @"{""Foo"":""Bar""}");

			var readOnlyDictionary = new ReadOnlyDictionary<string, string>(dictionary);
			Assert.AreEqual(JsonUtility.ToJson(readOnlyDictionary), @"{""Foo"":""Bar""}");
		}

		[Test]
		public void ExtraProperty()
		{
			Widget widget = null;
			const string jsonWithExtraProperty = @"{""kind"":{""weightInGrams"":2.4,""releaseDate"":""2010-11-12T13:14:15Z""},""title"":""whatever"",""version"":2}";
			Assert.Throws<JsonSerializationException>(() => widget = JsonUtility.FromJson<Widget>(jsonWithExtraProperty));
			widget = JsonUtility.FromJson<Widget>(jsonWithExtraProperty, new JsonInputSettings { IgnoresExtraProperties = true });
			Assert.AreEqual(widget.Title, "whatever");
			Assert.AreEqual(widget.Kind.ReleaseDate.Second, 15);
		}

		[TestCase(@"{""kind"":{""weightInGrams"":2.4,""releaseDate"":""2010-11-12T13:14:15Z""},""title"":""whatever""}", JTokenType.Object)]
		[TestCase(@"{}", JTokenType.Object)]
		[TestCase(@"[3,1,4,1,5,9]", JTokenType.Array)]
		[TestCase(@"[]", JTokenType.Array)]
		[TestCase(@"""hello""", JTokenType.String)]
		[TestCase(@"""2010-11-12T13:14:15Z""", JTokenType.String)]
		[TestCase(@"""e6f23c17-37bb-46a4-9ee8-2ea359377dab""", JTokenType.String)]
		[TestCase(@"""00:00:01""", JTokenType.String)]
		[TestCase(@"""http://www.logos.com/""", JTokenType.String)]
		[TestCase(@"3.14159", JTokenType.Float)]
		[TestCase(@"314159", JTokenType.Integer)]
		[TestCase(@"false", JTokenType.Boolean)]
		[TestCase(@"null", JTokenType.Null)]
		public void UsingJToken(string jsonFrom, JTokenType jTokenType)
		{
			JToken jToken = JsonUtility.FromJson<JToken>(jsonFrom);
			Assert.AreEqual(jToken.Type, jTokenType);
			Assert.AreEqual(JsonUtility.ToJson(jToken), jsonFrom);
		}

		[TestCase(@"null", @"null", true)]
		[TestCase(@"null", @"false", false)]
		[TestCase(@"false", @"null", false)]
		[TestCase(@"true", @"false", false)]
		[TestCase(@"true", @"true", true)]
		[TestCase(@"false", @"0", false)]
		[TestCase(@"0", @"0", true)]
		[TestCase(@"1.2", @"1.2", true)]
		[TestCase(@"1.2", @"1.0", false)]
		[TestCase(@"1.2", @"1", false)]
		[TestCase(@"{""baz"":""qux"",""foo"":""bar""}", @"{""foo"":""bar"",""baz"":""qux""}", true)]
		[TestCase(@"{""baz"":""qux"",""foo"":""baz""}", @"{""foo"":""bar"",""baz"":""qux""}", false)]
		[TestCase(@"{""baz"":""qux""}", @"{""foo"":""bar"",""baz"":""qux""}", false)]
		[TestCase(@"{""baz"":""qux""}", @"{""baz"":""bar""}", false)]
		[TestCase(@"{""baz"":""qux""}", @"{""foo"":""bar""}", false)]
		[TestCase(@"[""baz"",""qux""]", @"[""baz"",""qux""]", true)]
		[TestCase(@"[""qux"",""baz""]", @"[""baz"",""qux""]", false)]
		[TestCase(@"[[[]]]", @"[[[]]]", true)]
		[TestCase(@"[[[]]]", @"[[]]", false)]
		public void JTokenEquality(string beforeJson, string afterJson, bool areEqual)
		{
			JToken before = JsonUtility.FromJson<JToken>(beforeJson);
			JToken after = JsonUtility.FromJson<JToken>(afterJson);
			Assert.AreEqual(JsonUtility.JTokenEqualityComparer.Equals(before, after), areEqual);
			Assert.AreEqual(JsonUtility.JTokenEqualityComparer.Equals(after, before), areEqual);
			if (areEqual)
				Assert.AreEqual(JsonUtility.JTokenEqualityComparer.GetHashCode(before), JsonUtility.JTokenEqualityComparer.GetHashCode(after));
		}

		[Test]
		public void JTokenNullEquality()
		{
			// check weird edge cases
			Assert.IsTrue(JsonUtility.JTokenEqualityComparer.Equals(null, null));
			Assert.IsTrue(JsonUtility.JTokenEqualityComparer.Equals(new JValue((object) null), null));
			Assert.IsTrue(JsonUtility.JTokenEqualityComparer.Equals(new JValue((string) null), null));
			Assert.IsTrue(JsonUtility.JTokenEqualityComparer.Equals(new JValue((string) null), new JValue((object) null)));
		}

		[Test]
		public void JTokenPersistentHashCode()
		{
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(null), 10);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JValue((object) null)), 10);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JValue((string) null)), 10);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(false), -76102637);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(true), -142453839);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(""), 1028947972);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode("hi"), -1608206286);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(123), 567975604);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode((short) 123), 567975604);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode((long) 123), 567975604);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode((double) 123), 1436994632);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode((float) 123), 1436994632);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode((decimal) 123), 1436994632);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JObject()), 1430463807);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JObject { { "hi", 123 }, { "there", 456 } }), 2095528233);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JObject { { "there", 456 }, { "hi", 123 } }), 2095528233);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JArray()), -295539510);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JArray("hi", 123)), -1744639852);
			Assert.AreEqual(JsonUtility.GetPersistentHashCode(new JArray(123, "hi")), 1821679800);
		}

		private static void TestToAndFromJson<T>(T value, string json)
		{
			Assert.AreEqual(JsonUtility.ToJson(value), json);
			Assert.AreEqual(JsonUtility.FromJson<T>(json), value);
		}

		[Test]
		public void JsonNet40r7Bug()
		{
			HasNullableValue hasNullableValue = new HasNullableValue { Value = new SomeValue { Text = "hi" } };
			Assert.AreEqual(JsonUtility.ToJson(hasNullableValue), @"{""value"":""hi""}");
		}

		public class Widget
		{
			public string Title { get; set; }

			public WidgetKind Kind { get; set; }
		}

		public class WidgetKind
		{
			public double WeightInGrams { get; set; }

			public DateTime ReleaseDate { get; set; }
		}

		public class HasNullableValue
		{
			public SomeValue? Value { get; set; }
		}

		[JsonConverter(typeof(SomeValueJsonConverter))]
		public struct SomeValue
		{
			public string Text { get; set; }
		}

		public class SomeValueJsonConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return objectType == typeof(SomeValue) || objectType == typeof(SomeValue?);
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				SomeValue someValue = (SomeValue) value;
				writer.WriteValue(someValue.Text);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.Null)
					return null;
				return new SomeValue { Text = reader.Value.ToString() };
			}
		}
	}
}
