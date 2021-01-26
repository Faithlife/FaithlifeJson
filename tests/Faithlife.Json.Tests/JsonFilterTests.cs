using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Faithlife.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JsonFilterTests
	{
		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase(" ", "")]
		[TestCase("xyzzy", "xyzzy")]
		[TestCase(" xyzzy ", "xyzzy")]
		[TestCase("!xyzzy", "!xyzzy")]
		[TestCase("! xyzzy", "!xyzzy")]
		[TestCase("xy.zzy", "xy.zzy")]
		[TestCase(" xy . zzy ", "xy.zzy")]
		[TestCase("! xy . zzy", "!xy.zzy")]
		[TestCase("xy zzy", "xy zzy")]
		[TestCase("xy..zzy", null)]
		[TestCase("xy..zzy,abc", null)]
		[TestCase("xy.", null)]
		[TestCase(".zzy", null)]
		[TestCase(",abc,,def,", "abc,def")]
		[TestCase("abc,!def,ghi", "abc,!def,ghi")]
		[TestCase(" xy . zzy ,! foo . bar , foo,!abcd,!efg.hij.lmn ", "!abcd,!efg.hij.lmn,foo,!foo.bar,xy.zzy")]
		[TestCase("items.(id,(request,response.(code,content))),next", "items.id,items.request,items.response.code,items.response.content,next")]
		[TestCase("(me,you).(first,last)", "me.first,me.last,you.first,you.last")]
		[TestCase("re(quest,sponse)", "request,response")]
		[TestCase("a.(b,!c,!(d,!e.f))", "a.b,!a.c,!a.d,a.e.f")]
		[TestCase("xyzzy,!xyzzy", "")]
		[TestCase("!xyzzy,abc,xyzzy,!def", "abc,!def")]
		[TestCase("!xyzzy,xyzzy,!xyzzy", "!xyzzy")]
		[TestCase("xyzzy,!xyzzy,xyzzy", "xyzzy")]
		public void TryParseAndToString(string filterText, string roundTrip)
		{
			var filter = JsonFilter.TryParse(filterText);
			if (filter is null)
			{
				Assert.IsNull(roundTrip);
			}
			else
			{
				Assert.AreEqual(roundTrip, JsonFilter.JoinPaths(filter.GetPropertyPaths()));
				Assert.AreEqual(roundTrip, filter.ToString());
				Assert.AreEqual(roundTrip, JsonFilter.TryParse(filter.ToString())?.ToString());
			}
		}

		[Test]
		public void SupportSemicolons()
		{
			Assert.AreEqual(JsonFilter.TryParse("abc;!def,ghi;jkl")?.ToString(), "abc,!def,ghi,jkl");
		}

		[TestCase("xyzzy", "root.path", "*,root.*,root.path.xyzzy")]
		[TestCase("xyzzy", null, "xyzzy")]
		[TestCase("xyzzy", "", "xyzzy")]
		[TestCase("xyzzy", " ", null)]
		[TestCase("xyzzy", "root.path.", null)]
		[TestCase("xyzzy", "root..path", null)]
		[TestCase("xyzzy", ".root.path", null)]
		[TestCase("xyzzy", "root,path", null)]
		[TestCase(" xy . zzy ,! foo . bar , foo,!abcd,!efg.hij.lmn ", "root.path", "*,root.*,!root.path.abcd,!root.path.efg.hij.lmn,root.path.foo,!root.path.foo.bar,root.path.xy.zzy")]
		public void TryParseWithRootPath(string filterText, string rootPath, string roundTrip)
		{
			var filter = JsonFilter.TryParse(filterText, rootPath);
			if (filter is null)
			{
				Assert.IsNull(roundTrip);
			}
			else
			{
				Assert.AreEqual(roundTrip, filter.ToString());
				Assert.AreEqual(roundTrip, JsonFilter.TryParse(filter.ToString())?.ToString());
			}
		}

		[Test]
		public void BasicFilteredJsonWriterTest()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.None })
				using (JsonWriter filteredWriter = JsonFilter.Parse("name,!name.middle").CreateFilteredJsonWriter(jsonWriter))
					JToken.Parse(@"{""id"":123,""name"":{""first"":""Ed"",""middle"":""James"",""last"":""Ball""}}").WriteTo(filteredWriter);
				Assert.AreEqual(@"{""name"":{""first"":""Ed"",""last"":""Ball""}}", stringWriter.ToString());
			}
		}

		[Test]
		public void BasicFilterTokenTest()
		{
			var personBefore = new PersonDto { Id = 123, Name = new NameDto { First = "Ed", Middle = 'J', Last = "Ball" } };
			var personAfter = JsonFilter.Parse("name,!name.middle").FilterObject(personBefore)!;

			Assert.AreEqual(123, personBefore.Id);
			Assert.AreEqual(null, personAfter.Id);

			Assert.AreEqual("Ed", personBefore.Name.First);
			Assert.AreEqual("Ed", personAfter.Name!.First);

			Assert.AreEqual('J', personBefore.Name.Middle);
			Assert.AreEqual(null, personAfter.Name.Middle);

			Assert.AreEqual("Ball", personBefore.Name.Last);
			Assert.AreEqual("Ball", personAfter.Name.Last);
		}

		[Test]
		public void NullFilterTokenTest()
		{
			Assert.IsNull(JsonFilter.Parse("name,!name.middle").FilterToken(null));
			Assert.IsNull(JsonFilter.Parse("name,!name.middle").FilterObject((PersonDto?) null));
		}

		[Test, TestCaseSource(nameof(GetTestCases))]
		public void FilterTests(string filterText, string rootPath, string before, string after)
		{
			var filter = JsonFilter.TryParse(filterText, rootPath)!;
			JToken beforeToken = JToken.Parse(before);
			JToken afterToken = JToken.Parse(after);
			JToken? actualToken = filter.FilterToken(beforeToken);
			if (!JTokenUtility.AreEqual(actualToken, afterToken))
				Assert.Fail("expected {0} actual {1}", afterToken, actualToken);

			VerifyIsPathIncluded(filter, null, beforeToken, afterToken);
		}

		[Test]
		public void SimplePropertyPath()
		{
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Items), "items");
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Name), "name");
		}

		[Test]
		public void CustomNamePropertyPath()
		{
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.NextId), "next");
		}

		[Test]
		public void ChildPropertyPath()
		{
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Name!.First), "name.first");
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Name!.Middle), "name.middle");
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Name!.Last), "name.last");
		}

		[Test]
		public void ArrayPropertyPath()
		{
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Items![0].Id), "items.id");
			Assert.AreEqual(JsonFilter.GetPropertyPath((InterestingDto dto) => dto.Items![0].IsDeleted), "items.del");
		}

		[Test]
		public void JoinPropertyPaths()
		{
			JsonFilter.JoinPaths(
				JsonFilter.JoinPropertyPaths<InterestingDto>(dto => dto.Name, dto => dto.Items![0].Id),
				JsonFilter.JoinExcludedPropertyPaths<InterestingDto>(dto => dto.NextId, dto => dto.Items![0].IsDeleted), "name,items.id,!next,!items.del");
		}

		[Test]
		public void IsIncluded()
		{
			Assert.IsFalse(JsonFilter.Parse("id").IsPathIncluded("name.first"));
			Assert.IsTrue(JsonFilter.Parse("!id").IsPathIncluded("name.first"));
		}

		private void VerifyIsPathIncluded(JsonFilter filter, string? path, JToken beforeToken, JToken afterToken)
		{
			if (beforeToken is JObject beforeObject && afterToken is JObject afterObject)
			{
				foreach (var property in beforeObject)
				{
					string childPath = (path is null ? "" : (path + ".")) + property.Key;
					JToken afterChildToken;
					bool isIncluded = afterObject.TryGetValue(property.Key, out afterChildToken);
					Assert.AreEqual(filter.IsPathIncluded(childPath), isIncluded, "{0} was {1}", childPath, isIncluded ? "included" : "not included");

					VerifyIsPathIncluded(filter, childPath, property.Value, afterChildToken);
				}
			}
			else if (beforeToken is JArray beforeArray && afterToken is JArray afterArray && beforeArray.Count == afterArray.Count)
			{
				for (int index = 0; index < beforeArray.Count; index++)
					VerifyIsPathIncluded(filter, path, beforeArray[index], afterArray[index]);
			}
		}

		private sealed class PersonDto
		{
			public int? Id { get; set; }
			public NameDto? Name { get; set; }
		}

		private sealed class NameDto
		{
			public string? First { get; set; }
			public char? Middle { get; set; }
			public string? Last { get; set; }
		}

		private sealed class InterestingDto
		{
			public ReadOnlyCollection<InterestingItemDto>? Items { get; set; }

			[JsonProperty("next")]
			public string? NextId { get; set; }

			public NameDto? Name { get; set; }
		}

		private sealed class InterestingItemDto
		{
			public string? Id { get; set; }

			[JsonProperty("del")]
			public bool IsDeleted { get; set; }
		}

		private static IEnumerable<string?[]> GetTestCases()
		{
			Stream stream = typeof(JsonFilterTests).GetAssembly().GetManifestResourceStream("Faithlife.Json.Tests.JsonFilterTestCases.txt")!;
			Verify.IsNotNull(stream);
			using (var reader = new StreamReader(stream))
			{
				while (true)
				{
					string? filter;
					while (true)
					{
						filter = reader.ReadLine();
						if (filter is null)
							break;
						filter = filter.Trim();
						if (filter.Length != 0)
							break;
					}
					if (filter is null)
						break;

					string[] filterParts = filter.Split(new[] { '^' }, 2);
					yield return new[] { filterParts[filterParts.Length == 1 ? 0 : 1], filterParts.Length == 1 ? null : filterParts[0], reader.ReadLine(), reader.ReadLine() };
				}
			}
		}
	}
}
