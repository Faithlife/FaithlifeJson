namespace Faithlife.Json
{
	/// <summary>
	/// The kind of JSON patch operation.
	/// </summary>
	public enum JsonPatchOperationKind
	{
		/// <summary>
		/// Adds a node to the JSON.
		/// </summary>
		Add,

		/// <summary>
		/// Removes a node from the JSON.
		/// </summary>
		Remove,

		/// <summary>
		/// Replaces a node in the JSON.
		/// </summary>
		Replace
	}
}
