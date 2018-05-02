using System;

namespace Faithlife.Json
{
	/// <summary>
	/// Represents an value type for serialization/deserialization.
	/// </summary>
	public interface IValueTypeDto
	{
		/// <summary>
		/// Gets a value indicating whether the current IValueTypeDto object has a value.
		/// </summary>
		bool HasValue { get; }

		/// <summary>
		/// Gets the value of the current IValueTypeDto value.
		/// </summary>
		/// <exception cref="InvalidOperationException">The HasValue property is false.</exception>
		object Value { get; }

		/// <summary>
		/// Gets the type of the value that can be stored by this IValueTypeDto instance.
		/// </summary>
		/// <value>The type of the value that can be stored by this IValueTypeDto instance.</value>
		Type ValueType { get; }
	}
}
