using System;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class EnumAsIntegerJsonConverterTests
	{
		[Test]
		public void Serialize()
		{
			JsonConvert.SerializeObject(default(Enum?), new EnumAsIntegerJsonConverter()).ShouldBe("null");
			JsonConvert.SerializeObject(Enum.Test1, new EnumAsIntegerJsonConverter()).ShouldBe("1");
			JsonConvert.SerializeObject(FlagEnum.None, new EnumAsIntegerJsonConverter()).ShouldBe("0");
			JsonConvert.SerializeObject(FlagEnum.Flag1 | FlagEnum.Flag2 | FlagEnum.Flag3, new EnumAsIntegerJsonConverter()).ShouldBe("7");
			JsonConvert.SerializeObject(LittleEnum.Little0, new EnumAsIntegerJsonConverter()).ShouldBe("0");
			JsonConvert.SerializeObject(BigEnum.Big0, new EnumAsIntegerJsonConverter()).ShouldBe("2147483648");
			JsonUtility.ToJson(Enum.Test1, new JsonSettings { Converters = new[] { new EnumAsIntegerJsonConverter() } }).ShouldBe("1");
		}

		[Test]
		public void Deserialize()
		{
			JsonConvert.DeserializeObject<Enum>("1", new EnumAsIntegerJsonConverter()).ShouldBe(Enum.Test1);
			JsonConvert.DeserializeObject<Enum>("\"1\"", new EnumAsIntegerJsonConverter()).ShouldBe(Enum.Test1);
			JsonConvert.DeserializeObject<FlagEnum>("0", new EnumAsIntegerJsonConverter()).ShouldBe(FlagEnum.None);
			JsonConvert.DeserializeObject<LittleEnum>("1", new EnumAsIntegerJsonConverter()).ShouldBe(LittleEnum.Little1);
			JsonConvert.DeserializeObject<BigEnum>("2147483649", new EnumAsIntegerJsonConverter()).ShouldBe(BigEnum.Big1);
			JsonConvert.DeserializeObject<FlagEnum>("7", new EnumAsIntegerJsonConverter()).ShouldBe(FlagEnum.Flag1 | FlagEnum.Flag2 | FlagEnum.Flag3);
			JsonConvert.DeserializeObject<Enum?>("null", new EnumAsIntegerJsonConverter()).ShouldBe(null);
			JsonConvert.DeserializeObject<FlagEnum>("\"Flag1, Flag3\"", new EnumAsIntegerJsonConverter()).ShouldBe(FlagEnum.Flag1 | FlagEnum.Flag3);
			JsonConvert.DeserializeObject<FlagEnum>("\"flag2, flag3\"", new EnumAsIntegerJsonConverter()).ShouldBe(FlagEnum.Flag2 | FlagEnum.Flag3);
			JsonConvert.DeserializeObject<BigEnum>("\"big1\"", new EnumAsIntegerJsonConverter()).ShouldBe(BigEnum.Big1);
			JsonUtility.FromJson<Enum>("1", new JsonSettings { Converters = new[] { new EnumAsIntegerJsonConverter() } }).ShouldBe(Enum.Test1);
		}

		private enum Enum
		{
			Test1 = 1,
		}

		[Flags]
		private enum FlagEnum
		{
			None = 0,
			Flag1 = 1,
			Flag2 = 2,
			Flag3 = 4
		}

		private enum LittleEnum : short
		{
			Little0 = 0,
			Little1 = 1,
		}

		private enum BigEnum : long
		{
			Big0 = 0x80000000,
			Big1 = 0x80000001,
		}
	}
}
