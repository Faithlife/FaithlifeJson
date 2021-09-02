using System;
using System.ComponentModel;

namespace Faithlife.Json
{
	/// <summary>
	/// Sets the <c>DefaultValue</c> to <c>new T()</c> for the specified type.
	/// </summary>
	public sealed class DefaultValueDefaultAttribute : DefaultValueAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultValueDefaultAttribute"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		public DefaultValueDefaultAttribute(Type type)
			: base(Activator.CreateInstance(type))
		{
			Type = type;
		}

		/// <summary>
		/// The type.
		/// </summary>
		public Type Type { get; }
	}
}
