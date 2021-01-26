using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Faithlife.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json
{
	/// <summary>
	/// Utility methods for JToken.
	/// </summary>
	public static class JTokenUtility
	{
		/// <summary>
		/// An equality comparer for JToken.
		/// </summary>
		/// <remarks>This comparer ignores the order of object properties.</remarks>
		public static readonly IEqualityComparer<JToken?> EqualityComparer = new OurJTokenEqualityComparer();

		/// <summary>
		/// Check two JTokens for equality.
		/// </summary>
		/// <param name="left">The left JToken.</param>
		/// <param name="right">The right JToken.</param>
		/// <returns>True if the two JTokens are equal.</returns>
		public static bool AreEqual(JToken? left, JToken? right) => EqualityComparer.Equals(left, right);

		/// <summary>
		/// Returns a Boolean corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null or if it doesn't contain a Boolean.</returns>
		public static bool? AsBoolean(this JToken? jToken) => jToken is object && jToken.Type == JTokenType.Boolean ? (bool) jToken : default(bool?);

		/// <summary>
		/// Returns a Decimal corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a number,
		/// or if that number overflows a Decimal.</returns>
		public static decimal? AsDecimal(this JToken? jToken)
		{
			try
			{
				var jValue = jToken.AsNumber();
				return jValue is object ? (decimal) jValue : default(decimal?);
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
		public static double? AsDouble(this JToken? jToken)
		{
			try
			{
				var jValue = jToken.AsNumber();
				return jValue is object ? (double) jValue : default(double?);
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
		public static int? AsInt32(this JToken? jToken)
		{
			try
			{
				return jToken is object && jToken.Type == JTokenType.Integer ? (int) jToken : default(int?);
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
		public static long? AsInt64(this JToken? jToken)
		{
			try
			{
				return jToken is object && jToken.Type == JTokenType.Integer ? (long) jToken : default(long?);
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
		public static JValue? AsNumber(this JToken? jToken)
		{
			if (!(jToken is JValue jValue))
				return null;
			JTokenType jTokenType = jValue.Type;
			return jTokenType == JTokenType.Integer || jTokenType == JTokenType.Float ? jValue : null;
		}

		/// <summary>
		/// Returns a string corresponding to the JToken if possible.
		/// </summary>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain a string.</returns>
		public static string? AsString(this JToken? jToken) => jToken is object && jToken.Type == JTokenType.String ? (string) jToken : null;

		/// <summary>
		/// Returns true if the JToken is null or represents null.
		/// </summary>
		public static bool IsNull(this JToken? jToken)
		{
			if (jToken is null)
				return true;

			return jToken is JValue jValue && jValue.Value is null;
		}

		/// <summary>
		/// Returns the specified array item if possible.
		/// </summary>
		/// <param name="jToken">The JToken.</param>
		/// <param name="itemIndex">The index of the array item.</param>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain an array,
		/// or if the index is out of bounds.</returns>
		public static JToken? TryGetValue(this JToken? jToken, int itemIndex) => jToken is JArray jArray && itemIndex >= 0 && itemIndex < jArray.Count ? jArray[itemIndex] : null;

		/// <summary>
		/// Returns the specified property value if possible.
		/// </summary>
		/// <param name="jToken">The JToken.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>This method returns null if the JToken is null, or if it doesn't contain an object,
		/// or if the property name is null, or if the property doesn't exist.</returns>
		public static JToken? TryGetValue(this JToken? jToken, string? propertyName) => jToken is JObject jObject && propertyName is object ? jObject[propertyName] : null;

		/// <summary>
		/// Gets a persistent hash code for the token.
		/// </summary>
		/// <param name="token">The token, which must not be <c>null</c>.</param>
		/// <returns>The persistent hash code.</returns>
		public static int GetPersistentHashCode(JToken? token)
		{
			// return hard-coded hash code for null
			if (token.IsNull())
				return 10;

			JTokenType tokenType = token!.Type;

			// compare arrays
			if (tokenType == JTokenType.Array)
			{
				// combine type hash code with hash codes for elements
				IList<JToken> list = (IList<JToken>) token;
				int[] hashCodes = new int[list.Count + 1];
				hashCodes[0] = 2;
				for (int index = 0; index < list.Count; index++)
					hashCodes[index + 1] = GetPersistentHashCode(list[index]);
				return HashCodeUtility.CombineHashCodes(hashCodes);
			}

			// compare objects
			if (tokenType == JTokenType.Object)
			{
				// use XOR so that order doesn't matter
				IDictionary<string, JToken> properties = (IDictionary<string, JToken>) token;
				int hashCode = 0;
				foreach (KeyValuePair<string, JToken> property in properties)
					hashCode ^= HashCodeUtility.CombineHashCodes(property.Key.GetPersistentHashCode(), GetPersistentHashCode(property.Value));

				// combine type hash code with hash codes for properties
				return HashCodeUtility.CombineHashCodes(1, hashCode);
			}

			// combine type hash code with hash code for string
			if (tokenType == JTokenType.String)
				return HashCodeUtility.CombineHashCodes(8, ((string) token).GetPersistentHashCode());

			// combine type hash code with hash code for bool
			if (tokenType == JTokenType.Boolean)
				return HashCodeUtility.CombineHashCodes(9, HashCodeUtility.GetPersistentHashCode((bool) token));

			// return hash code of string representation of anything else (e.g. numbers)
			string tokenAsString = token.ToString(Formatting.None);
			return tokenAsString.GetPersistentHashCode();
		}

		/// <summary>
		/// Clones the specified Json.NET token.
		/// </summary>
		/// <typeparam name="T">The type of token.</typeparam>
		/// <param name="token">The token.</param>
		/// <returns>The clone.</returns>
		[return: NotNullIfNotNull("token")]
		public static T? Clone<T>(T? token)
			where T : JToken => (T?) token?.DeepClone();

		private sealed class OurJTokenEqualityComparer : IEqualityComparer<JToken?>
		{
			public bool Equals(JToken? left, JToken? right)
			{
				if (object.ReferenceEquals(left, right))
					return true;

				if (left.IsNull())
					return right.IsNull();
				else if (right.IsNull())
					return false;

				JTokenType leftType = left!.Type;
				JTokenType rightType = right!.Type;

				// compare arrays
				if (leftType == JTokenType.Array)
				{
					if (rightType != JTokenType.Array)
						return false;

					JArray leftArray = (JArray) left;
					JArray rightArray = (JArray) right;

					// check count then items
					return leftArray.Count == rightArray.Count && leftArray.SequenceEqual(rightArray, this);
				}

				// compare objects
				if (leftType == JTokenType.Object)
				{
					if (rightType != JTokenType.Object)
						return false;

					IDictionary<string, JToken> leftProperties = (IDictionary<string, JToken>) left;
					IDictionary<string, JToken> rightProperties = (IDictionary<string, JToken>) right;

					// check count first
					if (leftProperties.Count != rightProperties.Count)
						return false;

					// allow properties to be in any order, but make sure they have the same names and values
					foreach (KeyValuePair<string, JToken> leftProperty in leftProperties)
					{
						if (!rightProperties.TryGetValue(leftProperty.Key, out var rightValue))
							return false;
						if (!Equals(leftProperty.Value, rightValue))
							return false;
					}
					return true;
				}

				// compare strings ordinally
				if (leftType == JTokenType.String)
					return rightType == JTokenType.String && (string) left == (string) right;

				// compare Booleans
				if (leftType == JTokenType.Boolean)
					return rightType == JTokenType.Boolean && (bool) left == (bool) right;

				// compare string representations of anything else (e.g. numbers)
				return left.ToString(Formatting.None) == right.ToString(Formatting.None);
			}

			public int GetHashCode(JToken? token) => GetPersistentHashCode(token);
		}
	}
}
