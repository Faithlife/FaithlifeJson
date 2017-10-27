using System;
using System.IO;
using Faithlife.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class CamelCaseEnumJsonConverterTests
	{
		[Test]
		public void UsingStringComparisons()
		{
			JsonConvert.SerializeObject(StringComparison.Ordinal).ShouldBe("4");
			JsonConvert.SerializeObject(StringComparison.Ordinal, new StringEnumConverter()).ShouldBe("\"Ordinal\"");
			JsonConvert.SerializeObject(StringComparison.Ordinal, new StringEnumConverter { CamelCaseText = true }).ShouldBe("\"ordinal\"");
			JsonConvert.SerializeObject(StringComparison.Ordinal, new CamelCaseEnumJsonConverter()).ShouldBe("\"ordinal\"");
			JsonUtility.ToJson(StringComparison.Ordinal).ShouldBe("\"ordinal\"");

			JsonConvert.SerializeObject(StringComparison.OrdinalIgnoreCase).ShouldBe("5");
			JsonConvert.SerializeObject(StringComparison.OrdinalIgnoreCase, new StringEnumConverter()).ShouldBe("\"OrdinalIgnoreCase\"");
			JsonConvert.SerializeObject(StringComparison.OrdinalIgnoreCase, new StringEnumConverter { CamelCaseText = true }).ShouldBe("\"ordinalIgnoreCase\"");
			JsonConvert.SerializeObject(StringComparison.OrdinalIgnoreCase, new CamelCaseEnumJsonConverter()).ShouldBe("\"ordinalIgnoreCase\"");
			JsonUtility.ToJson(StringComparison.OrdinalIgnoreCase).ShouldBe("\"ordinalIgnoreCase\"");

			JsonConvert.DeserializeObject<StringComparison>("\"OrdinalIgnoreCase\"").ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"OrdinalIgnoreCase\"", new StringEnumConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"OrdinalIgnoreCase\"", new StringEnumConverter { CamelCaseText = true }).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"OrdinalIgnoreCase\"", new CamelCaseEnumJsonConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonUtility.FromJson<StringComparison>("\"OrdinalIgnoreCase\"").ShouldBe(StringComparison.OrdinalIgnoreCase);

			JsonConvert.DeserializeObject<StringComparison>("\"ordinalIgnoreCase\"").ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"ordinalIgnoreCase\"", new StringEnumConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"ordinalIgnoreCase\"", new StringEnumConverter { CamelCaseText = true }).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("\"ordinalIgnoreCase\"", new CamelCaseEnumJsonConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonUtility.FromJson<StringComparison>("\"ordinalIgnoreCase\"").ShouldBe(StringComparison.OrdinalIgnoreCase);

			JsonConvert.DeserializeObject<StringComparison>("5").ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("5", new StringEnumConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("5", new StringEnumConverter { CamelCaseText = true }).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonConvert.DeserializeObject<StringComparison>("5", new CamelCaseEnumJsonConverter()).ShouldBe(StringComparison.OrdinalIgnoreCase);
			JsonUtility.FromJson<StringComparison>("5").ShouldBe(StringComparison.OrdinalIgnoreCase);
		}

		[Test]
		public void UsingFileOptions()
		{
			JsonConvert.SerializeObject(FileOptions.RandomAccess | FileOptions.DeleteOnClose).ShouldBe("335544320");
			JsonConvert.SerializeObject(FileOptions.RandomAccess | FileOptions.DeleteOnClose, new StringEnumConverter()).ShouldBe("\"DeleteOnClose, RandomAccess\"");
			JsonConvert.SerializeObject(FileOptions.RandomAccess | FileOptions.DeleteOnClose, new StringEnumConverter { CamelCaseText = true }).ShouldBe("\"deleteOnClose, randomAccess\"");
			JsonConvert.SerializeObject(FileOptions.RandomAccess | FileOptions.DeleteOnClose, new CamelCaseEnumJsonConverter()).ShouldBe("\"deleteOnClose, randomAccess\"");
			JsonUtility.ToJson(FileOptions.RandomAccess | FileOptions.DeleteOnClose).ShouldBe("\"deleteOnClose, randomAccess\"");

			JsonConvert.DeserializeObject<FileOptions>("335544320").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("335544320", new StringEnumConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("335544320", new StringEnumConverter { CamelCaseText = true }).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("335544320", new CamelCaseEnumJsonConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonUtility.FromJson<FileOptions>("335544320").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);

			JsonConvert.DeserializeObject<FileOptions>("\"DeleteOnClose, RandomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"DeleteOnClose, RandomAccess\"", new StringEnumConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"DeleteOnClose, RandomAccess\"", new StringEnumConverter { CamelCaseText = true }).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"DeleteOnClose, RandomAccess\"", new CamelCaseEnumJsonConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonUtility.FromJson<FileOptions>("\"DeleteOnClose, RandomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);

			JsonConvert.DeserializeObject<FileOptions>("\"deleteOnClose, randomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"deleteOnClose, randomAccess\"", new StringEnumConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"deleteOnClose, randomAccess\"", new StringEnumConverter { CamelCaseText = true }).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\"deleteOnClose, randomAccess\"", new CamelCaseEnumJsonConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonUtility.FromJson<FileOptions>("\"deleteOnClose, randomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);

			JsonConvert.DeserializeObject<FileOptions>("\" deleteOnClose ,randomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\" deleteOnClose ,randomAccess\"", new StringEnumConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\" deleteOnClose ,randomAccess\"", new StringEnumConverter { CamelCaseText = true }).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonConvert.DeserializeObject<FileOptions>("\" deleteOnClose ,randomAccess\"", new CamelCaseEnumJsonConverter()).ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
			JsonUtility.FromJson<FileOptions>("\" deleteOnClose ,randomAccess\"").ShouldBe(FileOptions.RandomAccess | FileOptions.DeleteOnClose);
		}
	}
}
