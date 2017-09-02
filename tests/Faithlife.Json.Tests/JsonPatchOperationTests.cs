using System;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JsonPatchOperationTests
	{
		[TestCase(@"{""remove"":""/a/b/c""}", JsonPatchOperationKind.Remove, "/a/b/c", null)]
		[TestCase(@"{""add"":""/a/b/c"",""value"":123}", JsonPatchOperationKind.Add, "/a/b/c", "123")]
		[TestCase(@"{""replace"":""/a/b/c"",""value"":null}", JsonPatchOperationKind.Replace, "/a/b/c", "null")]
		[TestCase(@"{""add"":""/a/b/c"",""value"":true}", JsonPatchOperationKind.Add, "/a/b/c", "true")]
		[TestCase(@"{""replace"":""/a/b/c"",""value"":[]}", JsonPatchOperationKind.Replace, "/a/b/c", "[]")]
		[TestCase(@"{""add"":""/a/b/c"",""value"":{}}", JsonPatchOperationKind.Add, "/a/b/c", "{}")]
		[TestCase(@"{""replace"":""/a/b/c"",""value"":{""one"":1}}", JsonPatchOperationKind.Replace, "/a/b/c", @"{""one"":1}")]
		public void ValidJson(string json, JsonPatchOperationKind kind, string pointer, string value)
		{
			JsonPatchOperation op = JsonPatchOperation.FromJObject((JObject) JsonUtility.FromJson<JToken>(json));

			Assert.AreEqual(op.Kind, kind);
			Assert.AreEqual(op.Pointer.ToString(), pointer);
			if (value == null)
				Assert.IsNull(op.Value);
			else
				Assert.AreEqual(op.Value.ToString(Formatting.None), value);

			Assert.AreEqual(op.ToJObject().ToString(Formatting.None), json);
		}

		[TestCase(@"{""remove"":""/a/b/c"",""value"":123}")]
		[TestCase(@"{""add"":""/a/b/c""}")]
		[TestCase(@"{""replace"":""/a/b/c""}")]
		[TestCase(@"{""whatever"":""/a/b/c"",""value"":123}")]
		[TestCase(@"{}")]
		public void InvalidJson(string json)
		{
			Assert.Throws<ArgumentException>(() => JsonPatchOperation.FromJObject((JObject) JsonUtility.FromJson<JToken>(json)));
		}
	}
}
