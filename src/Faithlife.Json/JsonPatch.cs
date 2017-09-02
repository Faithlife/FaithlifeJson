using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Faithlife.Diff;
using Faithlife.Utility;
using Faithlife.Utility.Invariant;
using Newtonsoft.Json.Linq;

namespace Faithlife.Json
{
	/// <summary>
	/// Encapsulates a JSON patch.
	/// </summary>
	/// <remarks>See http://tools.ietf.org/html/draft-pbryan-json-patch-01 for details.</remarks>
	public sealed class JsonPatch
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatch"/> class.
		/// </summary>
		/// <param name="operations">The operations.</param>
		public JsonPatch(IEnumerable<JsonPatchOperation> operations)
			: this(operations, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatch"/> class.
		/// </summary>
		/// <param name="operations">The operations.</param>
		/// <param name="byteCountSaved">The approximate number of bytes saved.</param>
		public JsonPatch(IEnumerable<JsonPatchOperation> operations, int? byteCountSaved)
		{
			m_operations = operations.EmptyIfNull().ToList().AsReadOnly();
			m_byteCountSaved = byteCountSaved;
		}

		/// <summary>
		/// An empty, do-nothing JSON patch.
		/// </summary>
		public static readonly JsonPatch Empty = new JsonPatch(new JsonPatchOperation[0]);

		/// <summary>
		/// Creates a patch from a JSON array.
		/// </summary>
		/// <param name="operations">The array of operations.</param>
		/// <returns>The patch.</returns>
		public static JsonPatch FromJArray(JArray operations)
		{
			return new JsonPatch(operations.Cast<JObject>().Select(JsonPatchOperation.FromJObject));
		}

		/// <summary>
		/// Creates a JSON patch.
		/// </summary>
		/// <param name="before">The JSON before.</param>
		/// <param name="after">The JSON after.</param>
		/// <returns>The JSON patch.</returns>
		/// <remarks>To save time and memory, the portions of the 'after' token that are used in the patch are NOT cloned,
		/// and so should not be modified after the patch is created.</remarks>
		public static JsonPatch Create(JToken before, JToken after)
		{
			if (before == null)
				throw new ArgumentNullException("before");
			if (after == null)
				throw new ArgumentNullException("after");

			return DoCreate(before, after);
		}

		/// <summary>
		/// Gets the operations.
		/// </summary>
		/// <value>The operations.</value>
		public ReadOnlyCollection<JsonPatchOperation> Operations
		{
			get { return m_operations; }
		}

		/// <summary>
		/// The approximate number of bytes saved by the patch.
		/// </summary>
		/// <value>The approximate number of bytes saved by the patch. Null if unknown.</value>
		/// <remarks>If negative, the JSON of the patch uses more bytes than the JSON of the result
		/// of the patch.</remarks>
		public int? ByteCountSaved
		{
			get { return m_byteCountSaved; }
		}

		/// <summary>
		/// Applies the patch to the specified JSON token.
		/// </summary>
		/// <param name="before">The JSON token.</param>
		/// <returns>The result of applying the patch to the JSON token.</returns>
		/// <remarks>To save time and memory, the JSON tokens in the patch are NOT cloned when applied to the result,
		/// and so should not be modified after the patch is applied. The 'before' token is not modified when applying the patch.</remarks>
		public JToken Apply(JToken before)
		{
			JToken result = before.DeepClone();
			ApplyTo(ref result);
			return result;
		}

		/// <summary>
		/// Applies the patch to the specified JSON token.
		/// </summary>
		/// <param name="token">The JSON token, changed or modified as a result of applying the patch.</param>
		/// <remarks>To save time and memory, the JSON tokens in the patch are NOT cloned when applied to the result,
		/// and so should not be modified after the patch is applied. Also, when possible, the token is modified in place.</remarks>
		public void ApplyTo(ref JToken token)
		{
			foreach (JsonPatchOperation operation in m_operations)
			{
				bool remove = operation.Kind == JsonPatchOperationKind.Remove || operation.Kind == JsonPatchOperationKind.Replace;
				bool add = operation.Kind == JsonPatchOperationKind.Add || operation.Kind == JsonPatchOperationKind.Replace;

				if (operation.Pointer.Names.Count == 0)
				{
					if (!add || !remove)
						throw new JsonPatchException("The root can only be replaced.");
					token = operation.Value;
				}
				else
				{
					JsonPointer parentPointer = operation.Pointer.Parent;
					JToken parentToken = parentPointer.Evaluate(token);
					string lastName = operation.Pointer.Names.Last();

					JObject parentObject;
					JArray parentArray;
					if ((parentObject = parentToken as JObject) != null)
					{
						if (remove)
						{
							if (!parentObject.Remove(lastName))
								throw new JsonPatchException("{0} was not found on {1}.".FormatInvariant(lastName, parentPointer));
						}

						if (add)
						{
							try
							{
								parentObject.Add(lastName, operation.Value);
							}
							catch (ArgumentException x)
							{
								throw new JsonPatchException("{0} could not be added to {1}.".FormatInvariant(lastName, parentPointer), x);
							}
						}
					}
					else if ((parentArray = parentToken as JArray) != null)
					{
						int count = parentArray.Count;

						int index;
						if (!int.TryParse(lastName, NumberStyles.None, CultureInfo.InvariantCulture, out index) || index < 0)
							throw new JsonPatchException("{0} is not a valid array index.".FormatInvariant(lastName));

						if (remove)
						{
							if (index < 0 || index >= count)
								throw new JsonPatchException("{0} was not found on {1}.".FormatInvariant(index, parentPointer));

							parentArray.RemoveAt(index);
							count--;
						}

						if (add)
						{
							if (index < 0 || index > count)
								throw new JsonPatchException("{0} is out of range for {1}.".FormatInvariant(index, parentPointer));

							parentArray.Insert(index, operation.Value);
						}
					}
					else
					{
						throw new JsonPatchException("Patch can only apply to an object or an array. Parent of pointer {0} refers to a {1}."
							.FormatInvariant(operation.Pointer, parentToken != null ? parentToken.GetType().Name : "nothing"));
					}
				}
			}
		}

		/// <summary>
		/// Converts the patch to a JSON array.
		/// </summary>
		/// <returns></returns>
		public JArray ToJArray()
		{
			return new JArray(m_operations.Select(x => x.ToJObject()));
		}

		private static JsonPatch DoCreate(JToken before, JToken after)
		{
			const int byteCountSavedByReplace = -24; // {"replace":"","value":},

			// must replace if types aren't equal
			JTokenType tokenType = before.Type;
			if (tokenType == after.Type)
			{
				// special logic for objects and arrays
				if (tokenType == JTokenType.Object)
				{
					// prepare new patch
					List<JsonPatchOperation> operations = new List<JsonPatchOperation>();
					int? byteCountSaved = 0;

					// get before and after properties
					IDictionary<string, JToken> beforeProperties = (IDictionary<string, JToken>) before;
					IDictionary<string, JToken> afterProperties = (IDictionary<string, JToken>) after;

					// walk before properties
					Dictionary<string, JToken> afterPropertiesRemaining = new Dictionary<string, JToken>(afterProperties);
					foreach (KeyValuePair<string, JToken> beforeProperty in beforeProperties)
					{
						// point to this property
						JsonPointer pointer = new JsonPointer(new[] { beforeProperty.Key });

						// check for after property
						JToken afterValue;
						if (afterPropertiesRemaining.TryGetValue(beforeProperty.Key, out afterValue))
						{
							// patch the after property
							JsonPatch patch = DoCreate(beforeProperty.Value, afterValue);
							operations.AddRange(patch.Operations
								.Select(op => new JsonPatchOperation(op.Kind, pointer.Concat(op.Pointer), op.Value)));
							byteCountSaved += patch.ByteCountSaved;
							byteCountSaved += 4 + beforeProperty.Key.Length; // "name":,
							byteCountSaved -= (beforeProperty.Key.Length + 1) * patch.Operations.Count; // pointers get longer

							// done with after property
							afterPropertiesRemaining.Remove(beforeProperty.Key);
						}
						else
						{
							// no matching after property, so remove this before property
							operations.Add(new JsonPatchOperation(JsonPatchOperationKind.Remove, pointer));
							byteCountSaved -= 15 + beforeProperty.Key.Length; // {"remove":"/name"},
						}
					}

					// walk after properties that didn't match before properties
					foreach (KeyValuePair<string, JToken> afterProperty in afterPropertiesRemaining)
					{
						// no matching before property, so add this after property
						JsonPointer pointer = new JsonPointer(new[] { afterProperty.Key });
						operations.Add(new JsonPatchOperation(JsonPatchOperationKind.Add, pointer, afterProperty.Value));
						byteCountSaved -= 18; // {"add":/,"value"},
					}

					// return patch if it saves space
					if (byteCountSaved >= byteCountSavedByReplace)
						return new JsonPatch(operations, byteCountSaved);
				}
				else if (tokenType == JTokenType.Array)
				{
					// prepare new patch
					List<JsonPatchOperation> operations = new List<JsonPatchOperation>();
					int? byteCountSaved = 0;

					// get before and after items
					var beforeItems = ((IList<JToken>) before).AsReadOnlyList();
					var afterItems = ((IList<JToken>) after).AsReadOnlyList();

					// find differences, end to start so that changes near the start don't break the indices near the end
					var diffs = DiffUtility.FindDifferences(beforeItems, afterItems, JsonUtility.JTokenEqualityComparer).AsReadOnlyList();
					int identicalItemsIndex = afterItems.Count - 1;
					for (int diffIndex = diffs.Count - 1; diffIndex >= 0; diffIndex--)
					{
						var diff = diffs[diffIndex];

						// add byte savings for identical items
						for (int index = identicalItemsIndex; index >= diff.SecondRange.Start + diff.SecondRange.Length; index--)
							byteCountSaved += JsonUtility.ToJsonByteCount(afterItems[index]) + 1;
						identicalItemsIndex = diff.SecondRange.Start - 1;

						// add/remove extra items
						int addRemoveItemCount = diff.SecondRange.Length - diff.FirstRange.Length;
						int replaceItemCount = Math.Min(diff.SecondRange.Length, diff.FirstRange.Length);
						if (addRemoveItemCount > 0)
						{
							// add extra items
							for (int index = addRemoveItemCount - 1; index >= 0; index--)
							{
								int pointerIndex = diff.FirstRange.Start + replaceItemCount;
								string pointerString = pointerIndex.ToInvariantString();
								JsonPointer pointer = new JsonPointer(new[] { pointerString });
								JToken extraItem = afterItems[diff.SecondRange.Start + replaceItemCount + index];
								operations.Add(new JsonPatchOperation(JsonPatchOperationKind.Add, pointer, extraItem));
								byteCountSaved -= 21 + pointerString.Length; // {"add":"/123","value":},
							}
						}
						else if (addRemoveItemCount < 0)
						{
							// remove extra items
							for (int index = -addRemoveItemCount - 1; index >= 0; index--)
							{
								int pointerIndex = diff.FirstRange.Start + replaceItemCount + index;
								string pointerString = pointerIndex.ToInvariantString();
								JsonPointer pointer = new JsonPointer(new[] { pointerString });
								operations.Add(new JsonPatchOperation(JsonPatchOperationKind.Remove, pointer));
								byteCountSaved -= 15 + pointerString.Length; // {"remove":"/123"},
							}
						}

						// replace the rest
						for (int index = replaceItemCount - 1; index >= 0; index--)
						{
							// replace before with patch to after
							int pointerIndex = diff.FirstRange.Start + index;
							JsonPatch patch = DoCreate(beforeItems[pointerIndex], afterItems[diff.SecondRange.Start + index]);
							string pointerString = pointerIndex.ToInvariantString();
							JsonPointer pointer = new JsonPointer(new[] { pointerString });
							operations.AddRange(patch.Operations.Select(
								op => new JsonPatchOperation(op.Kind, pointer.Concat(op.Pointer), op.Value)));
							byteCountSaved += patch.ByteCountSaved;
							byteCountSaved += 1 + pointerString.Length; // 123,
							byteCountSaved -= (pointerString.Length + 1) * patch.Operations.Count; // pointers get longer
						}
					}

					// add byte savings for identical items at the end
					for (int index = identicalItemsIndex; index >= 0; index--)
						byteCountSaved += JsonUtility.ToJsonByteCount(afterItems[index]) + 1;

					// return patch if it saves space
					if (byteCountSaved >= byteCountSavedByReplace)
						return new JsonPatch(operations, byteCountSaved);
				}
				else if (JsonUtility.JTokenEqualityComparer.Equals(before, after))
				{
					// tokens are equal; no patch necessary
					int byteCountSaved = JsonUtility.ToJsonByteCount(after);
					return new JsonPatch(null, byteCountSaved);
				}
			}

			// if all else fails, replace
			return new JsonPatch(
				new[] { new JsonPatchOperation(JsonPatchOperationKind.Replace, JsonPointer.Root, after) }, byteCountSavedByReplace);
		}

		readonly ReadOnlyCollection<JsonPatchOperation> m_operations;
		readonly int? m_byteCountSaved;
	}
}
