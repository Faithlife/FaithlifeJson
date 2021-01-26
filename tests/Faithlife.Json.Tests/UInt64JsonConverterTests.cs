using System.IO;
using Faithlife.Json.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class UInt64JsonConverterTests
	{
		[Test]
		public void Serialize()
		{
			const ulong longMaxValue = long.MaxValue;
			JsonConvert.SerializeObject(null, new UInt64JsonConverter()).ShouldBe("null");
			JsonConvert.SerializeObject(ulong.MinValue, new UInt64JsonConverter()).ShouldBe("\"0\"");
			JsonConvert.SerializeObject(longMaxValue, new UInt64JsonConverter()).ShouldBe("\"9223372036854775807\"");
			JsonConvert.SerializeObject(longMaxValue + 1, new UInt64JsonConverter()).ShouldBe("\"9223372036854775808\"");
			JsonConvert.SerializeObject(ulong.MaxValue, new UInt64JsonConverter()).ShouldBe("\"18446744073709551615\"");
		}

		[Test]
		public void Deserialize()
		{
			const ulong longMaxValue = long.MaxValue;
			JsonConvert.DeserializeObject<ulong?>("", new UInt64JsonConverter()).ShouldBe(null);
			JsonConvert.DeserializeObject<ulong?>("null", new UInt64JsonConverter()).ShouldBe(null);
			JsonConvert.DeserializeObject<ulong>("\"0\"", new UInt64JsonConverter()).ShouldBe(0UL);
			JsonConvert.DeserializeObject<ulong>("0", new UInt64JsonConverter()).ShouldBe(0UL);
			JsonConvert.DeserializeObject<ulong>("\"9223372036854775807\"", new UInt64JsonConverter()).ShouldBe(longMaxValue);
			JsonConvert.DeserializeObject<ulong>("9223372036854775807", new UInt64JsonConverter()).ShouldBe(longMaxValue);
			JsonConvert.DeserializeObject<ulong>("\"9223372036854775808\"", new UInt64JsonConverter()).ShouldBe(longMaxValue + 1);
			JsonConvert.DeserializeObject<ulong>("\"18446744073709551615\"", new UInt64JsonConverter()).ShouldBe(ulong.MaxValue);
		}

		[Test]
		public void WriteNullValue()
		{
			UInt64JsonConverter converter = new UInt64JsonConverter();
			using (StringWriter stringWriter = new StringWriter())
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
				converter.WriteJson(jsonTextWriter, null, new JsonSerializer());
		}
	}
}
