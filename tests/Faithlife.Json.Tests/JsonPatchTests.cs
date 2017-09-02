using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JsonPatchTests
	{
		[TestCase(@"{""foo"":""bar""}", @"[]", @"{""foo"":""bar""}")]
		[TestCase(@"{""foo"":""bar""}", @"[{""add"":""/baz"",""value"":""qux""}]", @"{""baz"":""qux"",""foo"":""bar""}")]
		[TestCase(@"{""foo"":[""bar"",""baz""]}", @"[{""add"":""/foo/0"",""value"":""qux""}]", @"{""foo"":[""qux"",""bar"",""baz""]}")]
		[TestCase(@"{""foo"":[""bar"",""baz""]}", @"[{""add"":""/foo/1"",""value"":""qux""}]", @"{""foo"":[""bar"",""qux"",""baz""]}")]
		[TestCase(@"{""foo"":[""bar"",""baz""]}", @"[{""add"":""/foo/2"",""value"":""qux""}]", @"{""foo"":[""bar"",""baz"",""qux""]}")]
		[TestCase(@"{""baz"":""qux"",""foo"":""bar""}", @"[{""remove"":""/baz""}]", @"{""foo"":""bar""}")]
		[TestCase(@"{""baz"":""qux"",""foo"":""bar""}", @"[{""replace"":""/baz"",""value"":""boo""}]", @"{""baz"":""boo"",""foo"":""bar""}")]
		[TestCase(@"{""foo"":[{""bar"":[12]}]}", @"[{""replace"":""/foo/0/bar/0"",""value"":[false]}]", @"{""foo"":[{""bar"":[[false]]}]}")]
		[TestCase(@"{""foo"":""bar""}", @"[{""replace"":"""",""value"":""qux""}]", @"""qux""")]
		public void ApplyPatch(string beforeJson, string patchJson, string afterJson)
		{
			JToken before = JsonUtility.FromJson<JToken>(beforeJson);
			JsonPatch patch = JsonPatch.FromJArray(JArray.Parse(patchJson));
			JToken patched = patch.Apply(before);
			JToken after = JsonUtility.FromJson<JToken>(afterJson);
			Assert.That(patched, Is.EqualTo(after).Using(JsonUtility.JTokenEqualityComparer));
		}

		[TestCase(@"{""foo"":""bar""}", @"[{""add"":""/foo"",""value"":""qux""}]")]
		[TestCase(@"{""foo"":""bar""}", @"[{""add"":""/foo/foo"",""value"":""qux""}]")]
		[TestCase(@"{""foo"":[""bar"",""baz""]}", @"[{""add"":""/foo/3"",""value"":""qux""}]")]
		[TestCase(@"{""foo"":[""bar"",""baz""]}", @"[{""add"":""/foo/-1"",""value"":""qux""}]")]
		[TestCase(@"{""baz"":""qux"",""foo"":""bar""}", @"[{""remove"":""/qux""}]")]
		[TestCase(@"{""baz"":""qux"",""foo"":""bar""}", @"[{""replace"":""/qux"",""value"":""boo""}]")]
		[TestCase(@"{""foo"":""bar""}", @"[{""add"":"""",""value"":""qux""}]")]
		[TestCase(@"{""foo"":""bar""}", @"[{""remove"":""""}]")]
		public void FailedPatch(string beforeJson, string patchJson)
		{
			JToken before = JsonUtility.FromJson<JToken>(beforeJson);
			JsonPatch patch = JsonPatch.FromJArray(JArray.Parse(patchJson));
			Assert.Throws<JsonPatchException>(() => patch.Apply(before));
		}

		[TestCase(@"123", @"123", @"[]")]
		[TestCase(@"123", @"true", @"[{""replace"":"""",""value"":true}]")]
		[TestCase(@"{""foo"":""bar""}", @"{""foo"":""bar""}", @"[]")]
		[TestCase(@"{""foo"":""bar""}", @"{""foo"":""baz""}", @"[{""replace"":""/foo"",""value"":""baz""}]")]
		[TestCase(@"{""foo"":""bar"",""baz"":""qux""}", @"{""foo"":""bar""}", @"[{""remove"":""/baz""}]")]
		[TestCase(@"{""foo"":""bar"",""baz"":""qux""}", @"{""baz"":""qux""}", @"[{""remove"":""/foo""}]")]
		[TestCase(@"{""foo"":""bar"",""baz"":""qux""}", @"{""foo"":""bar"",""baz"":""boo""}", @"[{""replace"":""/baz"",""value"":""boo""}]")]
		[TestCase(@"{""foo"":""bar""}", @"{""foo"":""bar"",""baz"":""qux""}", @"[{""add"":""/baz"",""value"":""qux""}]")]
		[TestCase(@"{""foo"":""bar"",""noo"":""nar""}", @"{""foo"":""bar"",""noo"":""nar"",""baz"":""qux"",""boo"":""qoo""}",
			@"[{""add"":""/baz"",""value"":""qux""},{""add"":""/boo"",""value"":""qoo""}]")]
		[TestCase(@"{""foo0123456789"":""bar0123456789"",""noo0123456789"":""nar0123456789""}",
			@"{""foo0123456789"":""bar0123456789"",""noo0123456789"":""nar0123456789"",""baz0123456789"":""qux0123456789"",""boo0123456789"":""qoo0123456789""}",
			@"[{""add"":""/baz0123456789"",""value"":""qux0123456789""},{""add"":""/boo0123456789"",""value"":""qoo0123456789""}]")]
		[TestCase(@"{""foo"":{""bar"":12}}", @"{""foo"":{""bar"":false}}", @"[{""replace"":""/foo/bar"",""value"":false}]")]
		[TestCase(@"{""foo"":{""bar"":12}}", @"{""foo"":{""baz"":false}}", @"[{""replace"":""/foo"",""value"":{""baz"":false}}]")]
		[TestCase(@"[]", @"[]", @"[]")]
		[TestCase(@"[]", @"[12]", @"[{""add"":""/0"",""value"":12}]")]
		[TestCase(@"[12]", @"[]", @"[{""remove"":""/0""}]")]
		[TestCase(@"[12]", @"[24]", @"[{""replace"":""/0"",""value"":24}]")]
		[TestCase(@"[2,4]", @"[]", @"[{""replace"":"""",""value"":[]}]")]
		[TestCase(@"[2,4]", @"[6]", @"[{""replace"":"""",""value"":[6]}]")]
		[TestCase(@"[2,4]", @"[6,8]", @"[{""replace"":"""",""value"":[6,8]}]")]
		[TestCase(@"[2,4]", @"[6,8,10]", @"[{""replace"":"""",""value"":[6,8,10]}]")]
		[TestCase(@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006,2,4]",
			@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006]", @"[{""remove"":""/8""},{""remove"":""/7""}]")]
		[TestCase(@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006,2,4]",
			@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006,6]", @"[{""remove"":""/8""},{""replace"":""/7"",""value"":6}]")]
		[TestCase(@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006,2,4]",
			@"[1000000000,1000000001,1000000002,1000000003,1000000004,1000000005,1000000006,6,8]", @"[{""replace"":""/8"",""value"":8},{""replace"":""/7"",""value"":6}]")]
		[TestCase(@"[2,4,9,16,32]", @"[2,4,8,16,32]", @"[{""replace"":""/2"",""value"":8}]")]
		[TestCase(@"[[2],[4],[9],[16],[32]]", @"[[2],[4],[8],[16],[32]]", @"[{""replace"":""/2/0"",""value"":8}]")]
		[TestCase(@"[[1,2],[2,4],[3,9],[4,16],[5,32]]", @"[[1,2],[2,4],[3,8],[4,16],[5,32]]", @"[{""replace"":""/2/1"",""value"":8}]")]
		[TestCase(@"[{""n"":1,""v"":2},{""n"":2,""v"":4},{""n"":3,""v"":9},{""n"":4,""v"":16},{""n"":5,""v"":32}]",
			@"[{""n"":1,""v"":2},{""n"":2,""v"":4},{""n"":3,""v"":8},{""n"":4,""v"":16},{""n"":5,""v"":32}]",
			@"[{""replace"":""/2/v"",""value"":8}]")]
		[TestCase(@"[2,4,16,32,64,128,256]", @"[2,4,8,16,32,64,128,256]", @"[{""add"":""/2"",""value"":8}]")]
		[TestCase(@"[2,4,16,32,64,128,256]", @"[2,4,16,32,128,256]", @"[{""remove"":""/4""}]")]
		[TestCase(@"[2,4,16,32,64,128,256]", @"[2,4,8,16,32,128,256]", @"[{""remove"":""/4""},{""add"":""/2"",""value"":8}]")]
		[TestCase(@"[""this long string makes the patch worth creating this long string makes the patch worth creating this long string makes the patch worth creating""]", @"[""this long string makes the patch worth creating this long string makes the patch worth creating this long string makes the patch worth creating"",""2"",""3"",""4"",""5""]", @"[{""add"":""/1"",""value"":""5""},{""add"":""/1"",""value"":""4""},{""add"":""/1"",""value"":""3""},{""add"":""/1"",""value"":""2""}]")]
		[TestCase(@"[2000000000,4000000000,16000000000,32000000000,64000000000,128000000000,256000000000]", @"[2000000000,4000000000,8000000000,16000000000,32000000000,128000000000,256000000000]", @"[{""remove"":""/4""},{""add"":""/2"",""value"":8000000000}]")]
		[TestCase(@"[{""n"":1,""v"":2},{""n"":2,""v"":4},{""n"":4,""v"":16},{""n"":5,""v"":32},{""n"":6,""v"":64},{""n"":7,""v"":128},{""n"":8,""v"":256}]",
			@"[{""n"":1,""v"":2},{""n"":2,""v"":4},{""n"":3,""v"":8},{""n"":4,""v"":16},{""n"":5,""v"":32},{""n"":7,""v"":128},{""n"":8,""v"":256}]",
			@"[{""remove"":""/4""},{""add"":""/2"",""value"":{""n"":3,""v"":8}}]")]
		public void CreatePatch(string beforeJson, string afterJson, string patchJson)
		{
			JToken before = JsonUtility.FromJson<JToken>(beforeJson);
			JToken after = JsonUtility.FromJson<JToken>(afterJson);
			JsonPatch patch = JsonPatch.Create(before, after);
			Assert.AreEqual(patch.ToJArray(), JArray.Parse(patchJson));

			Assert.AreEqual(patch.Apply(before), after);

			// make sure byte count saved is close to reality
			int bytesSaved = afterJson.Length - patchJson.Length;
			Assert.LessOrEqual(System.Math.Abs(patch.ByteCountSaved.Value - bytesSaved), 4);
		}
	}
}
