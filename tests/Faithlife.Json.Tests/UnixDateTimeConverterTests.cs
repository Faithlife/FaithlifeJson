using System;
using Faithlife.Json.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class UnixDateTimeConverterTests
	{
		[Test]
		public void Serialize_MinValue()
		{
			DateTime utcMinValue = new DateTime(0, DateTimeKind.Utc);
			Assert.Throws<ArgumentOutOfRangeException>(() => JsonConvert.SerializeObject(utcMinValue, s_converter));
		}

		[Test]
		public void Serialize_TooOld()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => JsonConvert.SerializeObject(new DateTime(1600, 1, 1, 0, 0, 0, DateTimeKind.Utc), s_converter));
		}

		[Test]
		public void Serialize_InvalidKind()
		{
			Assert.Throws<ArgumentException>(() => JsonConvert.SerializeObject(DateTime.Now, s_converter));
		}

		[Test]
		public void RoundTrip()
		{
			DateTime d = new DateTime(1979, 7, 29, 11, 11, 11, DateTimeKind.Utc);

			string serializedValue = JsonConvert.SerializeObject(d, s_converter);
			DateTime deserializedValue = JsonConvert.DeserializeObject<DateTime>(serializedValue, s_converter);
			Assert.AreEqual(d, deserializedValue);
		}

		[Test]
		public void RoundTripNull()
		{
			DateTime? d = default(DateTime?);
			string serializedValue = JsonConvert.SerializeObject(d, s_converter);
			DateTime? deserializedValue = JsonConvert.DeserializeObject<DateTime?>(serializedValue, s_converter);
			Assert.AreEqual(d, deserializedValue);
		}

		[Test]
		public void Deserialize_NullWithInvalidType()
		{
			Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<DateTime>("null", s_converter));
		}

		private static readonly UnixDateTimeConverter s_converter = new UnixDateTimeConverter();
	}
}
