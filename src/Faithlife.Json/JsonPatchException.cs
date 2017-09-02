using System;

namespace Faithlife.Json
{
	/// <summary>
	/// A JSON patch exception.
	/// </summary>
	public class JsonPatchException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatchException"/> class.
		/// </summary>
		public JsonPatchException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatchException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public JsonPatchException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonPatchException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public JsonPatchException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
