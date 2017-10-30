using System;
using Faithlife.Json.Converters;
using Faithlife.Utility;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class OptionalJsonConverterTests
	{
		[Test]
		public void SaveAndLoad()
		{
			GoodOptionals optionals = new GoodOptionals();
			SaveAndLoad(optionals, @"{}");
			optionals.MyBoolean = false;
			SaveAndLoad(optionals, @"{""MyBoolean"":false}");
			optionals.MyBoolean = true;
			SaveAndLoad(optionals, @"{""MyBoolean"":true}");
			optionals.MyNullableBoolean = false;
			SaveAndLoad(optionals, @"{""MyBoolean"":true,""MyNullableBoolean"":false}");
			optionals.MyBoolean = default(Optional<bool>);
			optionals.MyNullableBoolean = true;
			SaveAndLoad(optionals, @"{""MyNullableBoolean"":true}");
			optionals.MyNullableBoolean = null;
			SaveAndLoad(optionals, @"{""MyNullableBoolean"":null}");
		}

		[Test]
		public void SaveBadOptionals()
		{
			BadOptionals optionals = new BadOptionals();
			SaveBad(optionals);
			optionals.MyBoolean = false;
			SaveBad(optionals);
			optionals.MyBoolean = true;
			SaveBad(optionals);
			optionals.MyNullableBoolean = false;
			SaveAndLoad(optionals, @"{""MyBoolean"":true,""MyNullableBoolean"":false}"); // don't fail because HasValue isn't false
			optionals.MyBoolean = default(Optional<bool>);
			optionals.MyNullableBoolean = true;
			SaveBad(optionals);
			optionals.MyNullableBoolean = null;
			SaveBad(optionals);
		}

		public struct GoodOptionals
		{
			[JsonProperty("MyBoolean", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(Optional<bool>))]
			public Optional<bool> MyBoolean { get; set; }

			[JsonProperty("MyNullableBoolean", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(Optional<bool?>))]
			public Optional<bool?> MyNullableBoolean { get; set; }
		}

		public struct BadOptionals
		{
			[JsonProperty("MyBoolean")]
			public Optional<bool> MyBoolean { get; set; }

			[JsonProperty("MyNullableBoolean")]
			public Optional<bool?> MyNullableBoolean { get; set; }
		}

		private void SaveAndLoad<T>(T optionals, string json)
		{
			Assert.AreEqual(json, JsonConvert.SerializeObject(optionals, new OptionalJsonConverter()), $"Failed: {json}");
			Assert.AreEqual(optionals, JsonConvert.DeserializeObject<T>(json, new OptionalJsonConverter()), $"Failed: {json}");

			Assert.AreEqual(json, JsonUtility.ToJson(optionals), $"Failed: {json}");
			Assert.AreEqual(optionals, JsonUtility.FromJson<T>(json), $"Failed: {json}");
		}

		private void SaveBad<T>(T optionals)
		{
			Assert.Throws<InvalidOperationException>(() => JsonConvert.SerializeObject(optionals, new OptionalJsonConverter()));
		}
	}
}
