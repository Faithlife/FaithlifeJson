using System;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json
{
	/// <summary>
	/// A JSON patch operation.
	/// </summary>
	public sealed class JsonPatchOperation
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatchOperation"/> class.
		/// </summary>
		/// <param name="kind">The kind.</param>
		/// <param name="pointer">The pointer.</param>
		public JsonPatchOperation(JsonPatchOperationKind kind, JsonPointer pointer)
			: this(kind, pointer, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatchOperation"/> class.
		/// </summary>
		/// <param name="kind">The kind.</param>
		/// <param name="pointer">The pointer.</param>
		/// <param name="value">The value.</param>
		public JsonPatchOperation(JsonPatchOperationKind kind, JsonPointer pointer, JToken value)
		{
			if (pointer == null)
				throw new ArgumentNullException("pointer");

			switch (kind)
			{
			case JsonPatchOperationKind.Add:
			case JsonPatchOperationKind.Replace:
				if (value == null)
					throw new ArgumentOutOfRangeException("value", "Value must not be null for Add or Replace. (Use new JValue((object) null) for null.)");
				break;
			case JsonPatchOperationKind.Remove:
				if (value != null)
					throw new ArgumentOutOfRangeException("value", "Value must be null for Remove.");
				break;
			default:
				throw new ArgumentOutOfRangeException("kind");
			}

			m_kind = kind;
			m_pointer = pointer;
			m_value = value;
		}

		/// <summary>
		/// Creates a JSON patch operation from the specified JSON object.
		/// </summary>
		/// <param name="obj">The JSON object.</param>
		/// <returns>The patch operation from the specified JSON object.</returns>
		public static JsonPatchOperation FromJObject(JObject obj)
		{
			JToken value = obj["value"];

			JToken remove = obj["remove"];
			if (remove != null && remove.Type == JTokenType.String && value == null)
				return new JsonPatchOperation(JsonPatchOperationKind.Remove, JsonPointer.Parse(((string) remove)));

			JToken add = obj["add"];
			if (add != null && add.Type == JTokenType.String && value != null)
				return new JsonPatchOperation(JsonPatchOperationKind.Add, JsonPointer.Parse((string) add), value.DeepClone());

			JToken replace = obj["replace"];
			if (replace != null && replace.Type == JTokenType.String && value != null)
				return new JsonPatchOperation(JsonPatchOperationKind.Replace, JsonPointer.Parse((string) replace), value.DeepClone());

			throw new ArgumentException("Invalid object.", "obj");
		}

		/// <summary>
		/// Gets the kind.
		/// </summary>
		/// <value>The kind.</value>
		public JsonPatchOperationKind Kind
		{
			get { return m_kind; }
		}

		/// <summary>
		/// Gets the pointer.
		/// </summary>
		/// <value>The pointer.</value>
		public JsonPointer Pointer
		{
			get { return m_pointer; }
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public JToken Value
		{
			get { return m_value; }
		}

		/// <summary>
		/// Converts the JSON patch operation to a JSON object.
		/// </summary>
		/// <returns></returns>
		public JObject ToJObject()
		{
			switch (m_kind)
			{
			case JsonPatchOperationKind.Add:
				return new JObject(new JProperty("add", m_pointer.ToString()), new JProperty("value", m_value));
			case JsonPatchOperationKind.Remove:
				return new JObject(new JProperty("remove", m_pointer.ToString()));
			case JsonPatchOperationKind.Replace:
				return new JObject(new JProperty("replace", m_pointer.ToString()), new JProperty("value", m_value));
			default:
				throw new InvalidOperationException();
			}
		}

		readonly JsonPatchOperationKind m_kind;
		readonly JsonPointer m_pointer;
		readonly JToken m_value;
	}
}
