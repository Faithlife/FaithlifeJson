using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JsonPointerTests
	{
		[TestCase(@"", new string[0], null)]
		[TestCase(@"/foo", new[] { "foo" }, null)]
		[TestCase(@"/foo/another%20prop", new[] { "foo", "another prop" }, @"/foo/another prop")]
		[TestCase(@"/foo/another%20prop/baz", new[] { "foo", "another prop", "baz" }, @"/foo/another prop/baz")]
		[TestCase(@"/foo/anArray/0", new[] { "foo", "anArray", "0" }, null)]
		[TestCase(@"/foo/has%2fslash", new[] { "foo", "has/slash" }, @"/foo/has%2Fslash")]
		public void ValidPointers(string pointerText, string[] names, string roundTrip)
		{
			JsonPointer pointer = JsonPointer.Parse(pointerText);
			CollectionAssert.AreEqual(pointer.Names, names);
			Assert.AreEqual(pointer.ToString(), roundTrip ?? pointerText);
		}

		[TestCase(@"/")]
		[TestCase(@"a/b/c")]
		[TestCase(@"/a//b/c")]
		[TestCase(@"/a/b/c/")]
		public void InvalidPointers(string pointerText)
		{
			JsonPointer pointer;
			Assert.IsFalse(JsonPointer.TryParse(pointerText, out pointer));

			Assert.Throws<FormatException>(() => JsonPointer.Parse(pointerText));
		}

		[TestCase(@"{""foo"":""bar""}", @"", @"{""foo"":""bar""}")]
		[TestCase(@"{""foo"":""bar""}", @"/foo", @"""bar""")]
		[TestCase(@"{""foo"":""bar""}", @"/bar", null)]
		[TestCase(@"{""foo"":""bar""}", @"/1", null)]
		[TestCase(@"[""foo"",""bar""]", @"", @"[""foo"",""bar""]")]
		[TestCase(@"[""foo"",""bar""]", @"/foo", null)]
		[TestCase(@"[""foo"",""bar""]", @"/0", @"""foo""")]
		[TestCase(@"[""foo"",""bar""]", @"/1", @"""bar""")]
		[TestCase(@"[""foo"",""bar""]", @"/2", null)]
		[TestCase(@"[""foo"",""bar""]", @"/-1", null)]
		[TestCase(@"{""foo"":[""bar"",{""foo"":[""bar""]}]}", @"/foo/0/foo/0", null)]
		[TestCase(@"{""foo"":[""bar"",{""foo"":[""bar""]}]}", @"/foo/1/foo/0", @"""bar""")]
		[TestCase(@"{""foo"":[""bar"",{""foo"":[""bar""]}]}", @"/foo/1/foo/1", null)]
		[TestCase(@"null", @"", @"null")]
		public void Evaluate(string jsonSource, string pointerText, string jsonTarget)
		{
			JsonPointer pointer = JsonPointer.Parse(pointerText);
			JToken jToken = pointer.Evaluate(JsonUtility.FromJson<JToken>(jsonSource));
			if (jToken == null)
				Assert.IsNull(jsonTarget);
			else
				Assert.AreEqual(JsonUtility.ToJson(jToken), jsonTarget);
		}
	}
}
