using NUnit.Framework;

namespace Faithlife.Json.Tests
{
	internal static class TestUtility
	{
		public static void ShouldBe<T>(this T actual, T expected)
		{
			Assert.AreEqual(expected, actual);
		}
	}
}
