using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using Faithlife.Utility;

namespace Faithlife.Json
{
	/// <summary>
	/// Points to a specific node within a JSON document.
	/// </summary>
	/// <remarks>See http://tools.ietf.org/html/draft-pbryan-zyp-json-pointer-00 for details.</remarks>
	public sealed class JsonPointer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPointer"/> class.
		/// </summary>
		/// <param name="names">The property names and/or array indices.</param>
		public JsonPointer(IEnumerable<string> names)
		{
			m_names = names.ToList().AsReadOnly();
		}

		/// <summary>
		/// Parses a JSON pointer.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>The JSON pointer.</returns>
		public static JsonPointer Parse(string text)
		{
			return DoParse(text, ThrowOnError.True);
		}

		/// <summary>
		/// Attempts to parse a JSON pointer.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="pointer">The JSON pointer.</param>
		/// <returns>True if successful.</returns>
		public static bool TryParse(string text, out JsonPointer pointer)
		{
			pointer = DoParse(text, ThrowOnError.False);
			return pointer != null;
		}

		/// <summary>
		/// The root pointer.
		/// </summary>
		public static readonly JsonPointer Root = new JsonPointer(new string[0]);

		/// <summary>
		/// Gets the property names and/or array indices.
		/// </summary>
		/// <value>The property names and/or array indices.</value>
		public ReadOnlyCollection<string> Names
		{
			get { return m_names; }
		}

		/// <summary>
		/// Gets the parent JSON pointer, or null if this pointer is at the root.
		/// </summary>
		/// <value>The parent JSON pointer, or null if this pointer is at the root.</value>
		public JsonPointer Parent
		{
			get { return m_names.Count == 0 ? null : new JsonPointer(m_names.Take(m_names.Count - 1)); }
		}

		/// <summary>
		/// Concatenates two JSON pointers.
		/// </summary>
		/// <param name="pointer">The other pointer.</param>
		/// <returns>The two JSON pointers, concatenated.</returns>
		public JsonPointer Concat(JsonPointer pointer)
		{
			if (pointer == null)
				throw new ArgumentNullException("pointer");

			return new JsonPointer(Names.Concat(pointer.Names));
		}

		/// <summary>
		/// Evaluates the pointer against the specified JSON token.
		/// </summary>
		/// <param name="jToken">The JSON token.</param>
		/// <returns>The JSON token being pointed to, or null if the token does not exist.</returns>
		public JToken Evaluate(JToken jToken)
		{
			foreach (string name in m_names)
			{
				JObject jObject;
				JArray jArray;
				if ((jObject = jToken as JObject) != null)
				{
					jToken = jObject[name];
					if (jToken == null)
						return null;
				}
				else if ((jArray = jToken as JArray) != null)
				{
					int index;
					if (int.TryParse(name, out index) && index >= 0 && index < jArray.Count)
						jToken = jArray[index];
					else
						return null;
				}
				else
				{
					return null;
				}
			}

			return jToken;
		}

		/// <summary>
		/// Converts the JSON pointer to a string.
		/// </summary>
		/// <returns>A string that represents this instance.</returns>
		public override string ToString()
		{
			return m_names.Select(x => "/" + UrlEncoding.Encode(x, s_urlEncodingSettings)).Join();
		}

		private static JsonPointer DoParse(string text, ThrowOnError throwOnError)
		{
			if (text == null)
			{
				if (throwOnError == ThrowOnError.True)
					throw new ArgumentNullException("text");
				return null;
			}

			if (text.Length == 0)
				return JsonPointer.Root;

			if (text[0] != '/')
			{
				if (throwOnError == ThrowOnError.True)
					throw new FormatException("A non-root pointer must start with a slash.");
				return null;
			}

			List<string> names = text.Split(new[] { '/' }, StringSplitOptions.None).Select(x => UrlEncoding.Decode(x, s_urlEncodingSettings)).Skip(1).ToList();

			foreach (string name in names)
			{
				if (name.Length == 0)
				{
					if (throwOnError == ThrowOnError.True)
						throw new FormatException("None of the names may be empty.");
					return null;
				}
			}

			return new JsonPointer(names);
		}

		static readonly UrlEncodingSettings s_urlEncodingSettings = new UrlEncodingSettings { ShouldEncodeChar = ch => ch == '/', UppercaseHexDigits = true };

		readonly ReadOnlyCollection<string> m_names;
	}
}
