using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	[TestFixture]
	public class JTokenUtilityTests
	{
		[Test]
		public void AsBooleanTests()
		{
			Assert.IsNull(((JToken) null).AsBoolean());
			Assert.IsTrue(JToken.Parse("true").AsBoolean());
			Assert.IsFalse(JToken.Parse("false").AsBoolean());
			Assert.IsNull(JToken.Parse("null").AsBoolean());
			Assert.IsNull(JToken.Parse("0").AsBoolean());
			Assert.IsNull(JToken.Parse("1").AsBoolean());
			Assert.IsNull(JToken.Parse("-0.9").AsBoolean());
			Assert.IsNull(JToken.Parse("\"\"").AsBoolean());
			Assert.IsNull(JToken.Parse("\"true\"").AsBoolean());
			Assert.IsNull(JToken.Parse("{}").AsBoolean());
			Assert.IsNull(JToken.Parse("[]").AsBoolean());
		}

		[Test]
		public void AsDecimalTests()
		{
			Assert.IsNull(((JToken) null).AsDecimal());
			Assert.IsNull(JToken.Parse("true").AsDecimal());
			Assert.IsNull(JToken.Parse("false").AsDecimal());
			Assert.IsNull(JToken.Parse("null").AsDecimal());
			Assert.AreEqual(JToken.Parse("0").AsDecimal(), 0.0m);
			Assert.AreEqual(JToken.Parse("1").AsDecimal(), 1.0m);
			Assert.AreEqual(JToken.Parse("-0.9").AsDecimal(), -0.9m);
			Assert.IsNull(JToken.Parse("\"\"").AsDecimal());
			Assert.IsNull(JToken.Parse("\"3.14\"").AsDecimal());
			Assert.IsNull(JToken.Parse("{}").AsDecimal());
			Assert.IsNull(JToken.Parse("[]").AsDecimal());
			Assert.AreEqual(JToken.Parse("1E+2").AsDecimal(), 100m);
		}

		[Test]
		public void AsDoubleTests()
		{
			Assert.IsNull(((JToken) null).AsDouble());
			Assert.IsNull(JToken.Parse("true").AsDouble());
			Assert.IsNull(JToken.Parse("false").AsDouble());
			Assert.IsNull(JToken.Parse("null").AsDouble());
			Assert.AreEqual(JToken.Parse("0").AsDouble(), 0.0);
			Assert.AreEqual(JToken.Parse("1").AsDouble(), 1.0);
			Assert.AreEqual(JToken.Parse("-0.9").AsDouble(), -0.9);
			Assert.IsNull(JToken.Parse("\"\"").AsDouble());
			Assert.IsNull(JToken.Parse("\"3.14\"").AsDouble());
			Assert.IsNull(JToken.Parse("{}").AsDouble());
			Assert.IsNull(JToken.Parse("[]").AsDouble());
			Assert.AreEqual(JToken.Parse("1E+2").AsDouble(), 100.0);
		}

		[Test]
		public void AsInt32Tests()
		{
			Assert.IsNull(((JToken) null).AsInt32());
			Assert.IsNull(JToken.Parse("true").AsInt32());
			Assert.IsNull(JToken.Parse("false").AsInt32());
			Assert.IsNull(JToken.Parse("null").AsInt32());
			Assert.AreEqual(JToken.Parse("0").AsInt32(), 0);
			Assert.AreEqual(JToken.Parse("1").AsInt32(), 1);
			Assert.IsNull(JToken.Parse("-0.9").AsInt32());
			Assert.IsNull(JToken.Parse("\"\"").AsInt32());
			Assert.IsNull(JToken.Parse("\"1\"").AsInt32());
			Assert.IsNull(JToken.Parse("{}").AsInt32());
			Assert.IsNull(JToken.Parse("[]").AsInt32());
			Assert.IsNull(JToken.Parse("2147483648").AsInt32());
			Assert.IsNull(JToken.Parse("1E+2").AsInt32());
		}

		[Test]
		public void AsInt64Tests()
		{
			Assert.IsNull(((JToken) null).AsInt64());
			Assert.IsNull(JToken.Parse("true").AsInt64());
			Assert.IsNull(JToken.Parse("false").AsInt64());
			Assert.IsNull(JToken.Parse("null").AsInt64());
			Assert.AreEqual(JToken.Parse("0").AsInt64(), 0);
			Assert.AreEqual(JToken.Parse("1").AsInt64(), 1);
			Assert.IsNull(JToken.Parse("-0.9").AsInt64());
			Assert.IsNull(JToken.Parse("\"\"").AsInt64());
			Assert.IsNull(JToken.Parse("\"1\"").AsInt64());
			Assert.IsNull(JToken.Parse("{}").AsInt64());
			Assert.IsNull(JToken.Parse("[]").AsInt64());
			Assert.IsNull(JToken.Parse("1E+18").AsInt64());
		}

		[Test]
		public void AsNumberTests()
		{
			Assert.IsNull(((JToken) null).AsNumber());
			Assert.IsNull(JToken.Parse("true").AsNumber());
			Assert.IsNull(JToken.Parse("false").AsNumber());
			Assert.IsNull(JToken.Parse("null").AsNumber());
			Assert.AreEqual(JToken.Parse("0").AsNumber().Type, JTokenType.Integer);
			Assert.AreEqual(JToken.Parse("1").AsNumber().Type, JTokenType.Integer);
			Assert.AreEqual(JToken.Parse("-0.9").AsNumber().Type, JTokenType.Float);
			Assert.IsNull(JToken.Parse("\"\"").AsNumber());
			Assert.IsNull(JToken.Parse("\"1\"").AsNumber());
			Assert.IsNull(JToken.Parse("{}").AsNumber());
			Assert.IsNull(JToken.Parse("[]").AsNumber());
			Assert.AreEqual(JToken.Parse("1E+2").AsNumber().Type, JTokenType.Float);
		}

		[Test]
		public void AsStringTests()
		{
			Assert.IsNull(((JToken) null).AsString());
			Assert.IsNull(JToken.Parse("true").AsString());
			Assert.IsNull(JToken.Parse("false").AsString());
			Assert.IsNull(JToken.Parse("null").AsString());
			Assert.IsNull(JToken.Parse("0").AsString());
			Assert.IsNull(JToken.Parse("1").AsString());
			Assert.IsNull(JToken.Parse("-0.9").AsString());
			Assert.AreEqual(JToken.Parse("\"\"").AsString(), "");
			Assert.IsNull(JToken.Parse("{}").AsString());
			Assert.IsNull(JToken.Parse("[]").AsString());
			Assert.AreEqual(JToken.Parse("\"\u0000\"").AsString(), "\u0000");
			Assert.AreEqual(JToken.Parse("\"\\u0000\"").AsString(), "\u0000");
		}

		[Test]
		public void IsNullTests()
		{
			Assert.IsTrue(((JToken) null).IsNull());
			Assert.IsTrue(JToken.Parse("null").IsNull());

			Assert.IsTrue(new JValue((object) null).IsNull());
			Assert.IsTrue(new JValue((string) null).IsNull());
			Assert.IsTrue(JValue.CreateString(null).IsNull());
			Assert.IsTrue(((JToken) default(bool?)).IsNull());
			Assert.IsTrue(((JToken) default(decimal?)).IsNull());
			Assert.IsTrue(((JToken) default(double?)).IsNull());
			Assert.IsTrue(((JToken) default(int?)).IsNull());
			Assert.IsTrue(((JToken) default(long?)).IsNull());
			Assert.IsTrue(((JToken) default(float?)).IsNull());
			Assert.IsTrue(((JToken) default(bool?)).IsNull());

			Assert.IsFalse(JToken.Parse("false").IsNull());
			Assert.IsFalse(JToken.Parse("0").IsNull());
			Assert.IsFalse(JToken.Parse("\"\"").IsNull());
			Assert.IsFalse(JToken.Parse("{}").IsNull());
			Assert.IsFalse(JToken.Parse("[]").IsNull());
		}

		[Test]
		public void TryGetValueAtIndexTests()
		{
			Assert.IsNull(((JToken) null).TryGetValue(0));
			Assert.IsNull(JToken.Parse("true").TryGetValue(0));
			Assert.IsNull(JToken.Parse("false").TryGetValue(0));
			Assert.IsNull(JToken.Parse("null").TryGetValue(0));
			Assert.IsNull(JToken.Parse("0").TryGetValue(0));
			Assert.IsNull(JToken.Parse("1").TryGetValue(0));
			Assert.IsNull(JToken.Parse("-0.9").TryGetValue(0));
			Assert.IsNull(JToken.Parse("\"\"").TryGetValue(0));
			Assert.IsNull(JToken.Parse("{}").TryGetValue(0));
			Assert.IsNull(JToken.Parse("[]").TryGetValue(0));
			Assert.AreEqual(JToken.Parse("[1]").TryGetValue(0).AsInt32(), 1);
			Assert.IsNull(JToken.Parse("[1]").TryGetValue(1));
			Assert.IsNull(JToken.Parse("[1]").TryGetValue(-1));
			Assert.AreEqual(JToken.Parse("[\"1\"]").TryGetValue(0).AsString(), "1");
			Assert.IsNull(JToken.Parse("[\"1\"]").TryGetValue(0).AsInt32());
		}

		[Test]
		public void TryGetValueAtNameTests()
		{
			Assert.IsNull(((JToken) null).TryGetValue("key"));
			Assert.IsNull(JToken.Parse("true").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("false").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("null").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("0").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("1").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("-0.9").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("\"\"").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("{}").TryGetValue("key"));
			Assert.IsNull(JToken.Parse("[]").TryGetValue("key"));
			Assert.AreEqual(JToken.Parse("{\"key\":1}").TryGetValue("key").AsInt32(), 1);
			Assert.IsNull(JToken.Parse("{\"key\":1}").TryGetValue("xyz"));
			Assert.IsNull(JToken.Parse("{\"key\":1}").TryGetValue(""));
			Assert.IsNull(JToken.Parse("{\"key\":1}").TryGetValue(null));
			Assert.AreEqual(JToken.Parse("{\"\":1}").TryGetValue("").AsInt32(), 1);
		}
	}
}
