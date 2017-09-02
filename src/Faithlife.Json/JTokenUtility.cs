using System;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json
{
	/// <summary>
	/// Utility methods for JToken.
	/// </summary>
	public static class JTokenUtility
	{
		/// <summary>
		/// Check two JTokens for equality.
		/// </summary>
		/// <param name="left">The left JToken.</param>
		/// <param name="right">The right JToken.</param>
		/// <returns>True if the two JTokens are equal.</returns>
		public static bool AreEqual(JToken left, JToken right)
		{
			return JsonUtility.JTokenEqualityComparer.Equals(left, right);
		}

		/// <summary>
		/// Returns a Boolean corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null or if it doesn't contain a Boolean.</returns>
		public static bool? AsBoolean(this JToken jToken)
		{
			return jToken != null && jToken.Type == JTokenType.Boolean ? (bool) jToken : default(bool?);
		}

		/// <summary>
		/// Returns a Decimal corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a number,
		/// or if that number overflows a Decimal.</returns>
		public static decimal? AsDecimal(this JToken jToken)
		{
			try
			{
				JValue jValue = jToken.AsNumber();
				return jValue != null ? (decimal) jValue : default(decimal?);
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns a Double corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a number,
		/// or if that number overflows a Double.</returns>
		public static double? AsDouble(this JToken jToken)
		{
			try
			{
				JValue jValue = jToken.AsNumber();
				return jValue != null ? (double) jValue : default(double?);
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns an Int32 corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a number,
		/// or if that number overflows an Int32, or if that number was parsed as floating-point.</returns>
		public static int? AsInt32(this JToken jToken)
		{
			try
			{
				return jToken != null && jToken.Type == JTokenType.Integer ? (int) jToken : default(int?);
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns an Int64 corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a number,
		/// or if that number overflows an Int64, or if that number was parsed as floating-point.</returns>
		public static long? AsInt64(this JToken jToken)
		{
			try
			{
				return jToken != null && jToken.Type == JTokenType.Integer ? (long) jToken : default(long?);
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		/// <summary>
		/// Returns the token as a JValue if it corresponds to a number.
		/// </summary>
		/// <returns>This method returns null if the JToken is null or if it doesn't contain a number.</returns>
		/// <remarks>Use this method to "filter out" non-numeric tokens without losing any precision
		/// on the number itself (since numeric representations of the number could lose precision).</remarks>
		public static JValue AsNumber(this JToken jToken)
		{
			JValue jValue = jToken as JValue;
			if (jValue == null)
				return null;
			JTokenType jTokenType = jValue.Type;
			return jTokenType == JTokenType.Integer || jTokenType == JTokenType.Float ? jValue : null;
		}

		/// <summary>
		/// Returns a string corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a string.</returns>
		public static string AsString(this JToken jToken)
		{
			return jToken != null && jToken.Type == JTokenType.String ? (string) jToken : null;
		}

		/// <summary>
		/// Returns true if the JToken is null or represents null.
		/// </summary>
		public static bool IsNull(this JToken jToken)
		{
			if (jToken == null)
				return true;

			JValue jValue = jToken as JValue;
			return jValue != null && jValue.Value == null;
		}

		/// <summary>
		/// Returns the specified array item if possible.
		/// </summary>
		/// <param name="jToken">The JToken.</param>
		/// <param name="itemIndex">The index of the array item.</param>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain an array,
		/// or if the index is out of bounds.</returns>
		public static JToken TryGetValue(this JToken jToken, int itemIndex)
		{
			JArray jArray = jToken as JArray;
			return jArray != null && itemIndex >= 0 && itemIndex < jArray.Count ? jArray[itemIndex] : null;
		}

		/// <summary>
		/// Returns the specified property value if possible.
		/// </summary>
		/// <param name="jToken">The JToken.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain an object,
		/// or if the property name is null, or if the property doesn't exist.</returns>
		public static JToken TryGetValue(this JToken jToken, string propertyName)
		{
			JObject jObject = jToken as JObject;
			return jObject != null && propertyName != null ? jObject[propertyName] : null;
		}
	}
}
