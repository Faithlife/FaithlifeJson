using System;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class ValueTypeDtoJsonConverterTests
	{
		[Test]
		public void ThrowsWhenNoValue()
		{
			Assert.Throws<InvalidOperationException>(() => { bool _ = new ValueTypeDto<bool>().Value; });
			Assert.Throws<InvalidOperationException>(() => { bool _ = new ValueTypeDto<bool>(); });
			Assert.Throws<InvalidOperationException>(() => { bool _ = default(ValueTypeDto<bool>).Value; });
			Assert.Throws<InvalidOperationException>(() => { bool _ = default(ValueTypeDto<bool>); });
		}

		[Test]
		public void ImplicitCastSuccess()
		{
			Assert.IsTrue(new ValueTypeDto<bool>(true).Value);
			Assert.IsTrue(new ValueTypeDto<bool>(true));

			var valueTypeDtoTest = new ValueTypeDtoTest { MyBoolean = true };
			bool myBoolean = valueTypeDtoTest.MyBoolean;
			Assert.IsTrue(myBoolean);
		}

		[TestCase(@"{""MyBoolean"":null}")]
		public void DeserializeFailureOnNullValueType(string json)
		{
			AssertDeserializationFailure<ValueTypeDtoTest>(json);
		}

		[TestCase(@"{""MyBoolean"":true}", true, true)]
		[TestCase(@"{""MyBoolean"":false}", true, false)]
		[TestCase(@"{}", false, null)]
		public void DeserializeSuccess(string json, bool expectedHasValue, bool? expectedValue)
		{
			var expectedValueTypeDto = !expectedHasValue
				? new ValueTypeDto<bool>()
				: new ValueTypeDto<bool>(expectedValue ?? throw new InvalidOperationException($"Invalid test case: \"{nameof(expectedValue)}\" may not be null when {nameof(expectedHasValue)} is true."));

			AssertDeserializationSuccess(json, new ValueTypeDtoTest { MyBoolean = expectedValueTypeDto });
		}

		[TestCase(false, null)]
		[TestCase(null, null)]
		public void SerializeFailureOnMissingAttributes(bool? hasValue, bool? value)
		{
			var valueTypeDtoTest = new ValueTypeDtoNoAttributesTest();
			if (hasValue.HasValue)
			{
				valueTypeDtoTest.MyBoolean = !hasValue.Value
					? new ValueTypeDto<bool>()
					: new ValueTypeDto<bool>(value ?? throw new InvalidOperationException($"Invalid test case: \"{nameof(value)}\" may not be null when {nameof(hasValue)} is true."));
			}

			AssertSerializationFailure(valueTypeDtoTest);
		}

		[TestCase(true, true, @"{""MyBoolean"":true}")]
		[TestCase(true, false, @"{""MyBoolean"":false}")]
		[TestCase(false, null, @"{}")]
		[TestCase(null, null, @"{}")]
		public void SerializeSuccess(bool? hasValue, bool? value, string expectedJson)
		{
			var valueTypeDtoTest = new ValueTypeDtoTest();
			if (hasValue.HasValue)
			{
				valueTypeDtoTest.MyBoolean = !hasValue.Value
					? new ValueTypeDto<bool>()
					: new ValueTypeDto<bool>(value ?? throw new InvalidOperationException($"Invalid test case: \"{nameof(value)}\" may not be null when {nameof(hasValue)} is true."));
			}

			AssertSerializationSuccess(valueTypeDtoTest, expectedJson);
		}

		public struct ValueTypeDtoTest
		{
			[JsonProperty("MyBoolean", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(ValueTypeDto<bool>))]
			public ValueTypeDto<bool> MyBoolean { get; set; }
		}

		public struct ValueTypeDtoNoAttributesTest
		{
			[JsonProperty("MyBoolean")]
			public ValueTypeDto<bool> MyBoolean { get; set; }
		}

		private static void AssertDeserializationFailure<T>(string json)
		{
			Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<T>(json, new ValueTypeDtoJsonConverter()));
			Assert.Throws<JsonSerializationException>(() => JsonUtility.FromJson<T>(json));
		}

		private static void AssertDeserializationSuccess<T>(string json, T expectedValue)
		{
			Assert.AreEqual(expectedValue, JsonConvert.DeserializeObject<T>(json, new ValueTypeDtoJsonConverter()), $"Failed: {json}");
			Assert.AreEqual(expectedValue, JsonUtility.FromJson<T>(json), $"Failed: {json}");
		}

		private static void AssertSerializationFailure<T>(T test)
		{
			Assert.Throws<InvalidOperationException>(() => JsonConvert.SerializeObject(test, new ValueTypeDtoJsonConverter()));
			Assert.Throws<InvalidOperationException>(() => JsonUtility.ToJson(test));
		}

		private static void AssertSerializationSuccess<T>(T value, string expectedJson)
		{
			Assert.AreEqual(expectedJson, JsonConvert.SerializeObject(value, new ValueTypeDtoJsonConverter()), $"Failed: {expectedJson}");
			Assert.AreEqual(expectedJson, JsonUtility.ToJson(value), $"Failed: {expectedJson}");
		}
	}
}
