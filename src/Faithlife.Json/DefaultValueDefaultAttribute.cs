using System;
using System.ComponentModel;

namespace Faithlife.Json
{
	/// <summary>
	/// Sets the DefaultValue to default(T) for the specified type.
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
		}
	}
}
