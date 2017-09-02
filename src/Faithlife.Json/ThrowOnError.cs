using System;

namespace Faithlife.Json
{
	/// <summary>
	/// Indicates whether a method should throw an exception if it fails.
	/// </summary>
	internal enum ThrowOnError
	{
		/// <summary>
		/// The method should not throw an exception if it fails.
		/// </summary>
		False,

		/// <summary>
		/// The method should throw an exception if it fails.
		/// </summary>
		True
	}

	/// <summary>
	/// Utility methods for ThrowOnError.
	/// </summary>
	internal static class ThrowOnErrorUtility
	{
		/// <summary>
		/// Throws the specified exception if required.
		/// </summary>
		/// <param name="throwOnError">The throw on error.</param>
		/// <param name="exception">The exception thrown if throwOnError is True.</param>
		/// <returns>False if throwOnError is False.</returns>
		/// <remarks>If performance may be an issue, do not use this method.</remarks>
		public static bool TryThrow(this ThrowOnError throwOnError, Exception exception)
		{
			if (throwOnError == ThrowOnError.True)
				throw exception;
			return false;
		}
	}
}
