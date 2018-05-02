using System;
using System.Collections.Generic;
using Faithlife.Json.Converters;
using Faithlife.Utility;

namespace Faithlife.Json
{
	/// <summary>
	/// Wraps a value type for JSON serialzation/deserialzation in order to handle missing values.
	/// When a value is not present in the JSON, a value type will normally be deserialized as the default value for that type.
	/// This class instead deserializes with HasValue = false and will throw and an exception if Value is accessed (implicitly or explicitly).
	/// Implicit casting allows usage of ValueTypeDto{T} just like <paramtyperef name="T"/>.
	/// </summary>
	/// <remarks>
	/// Use <see cref="Optional{T}"/> for serializing/deserializing optional ValueType values.
	/// Use <see cref="ValueTypeDto{T}"/> for serializing/deserializing required ValueType values.
	/// Note that <see cref="ValueTypeDto{T}"/> also supports "fields" handling when a value is not requested and therefore not populated.
	/// <see cref="ValueTypeDto{T}"/> should be used in conjunction with <see cref="ValueTypeDtoJsonConverter"/>.
	/// Any property of this type should use the following attributes:
	/// [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(ValueTypeDto&lt;bool&gt;))]
	/// </remarks>
	/// <example>
	/// * `{"MyBoolean":true}` -> true
	/// * `{"MyBoolean":false}` -> false
	/// * `{"MyBoolean":null}` -> Exception during deserialization (null does not deserialize to a boolean)
	/// * `{}` -> HasValue == false; exception if Value is accessed.
	/// </example>
	/// <typeparam name="T">The type of the value type.</typeparam>
	public struct ValueTypeDto<T> : IEquatable<ValueTypeDto<T>>, IValueTypeDto
		where T : struct
	{
		/// <summary>
		/// Initializes a new instance of the ValueTypeDto{T} structure to the specified value. 
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueTypeDto(T value)
		{
			m_value = value;
			m_hasValue = true;
		}

		/// <summary>
		/// Gets a value indicating whether the current ValueTypeDto{T} object has a value.
		/// </summary>
		public bool HasValue => m_hasValue;

		/// <summary>
		/// Gets the value of the current ValueTypeDto{T} value.
		/// </summary>
		/// <exception cref="InvalidOperationException">The HasValue property is false.</exception>
		public T Value
		{
			get
			{
				if (!m_hasValue)
					throw new InvalidOperationException("The HasValue property is false.");

				return m_value;
			}
		}

		/// <summary>
		/// Gets the value of the current ValueTypeDto{T} value.
		/// </summary>
		/// <exception cref="InvalidOperationException">The HasValue property is false.</exception>
		object IValueTypeDto.Value => Value;

		/// <summary>
		/// Gets the type of the value that can be stored by this IValueTypeDto instance.
		/// </summary>
		/// <value>The type of the value that can be stored by this IValueTypeDto instance.</value>
		Type IValueTypeDto.ValueType => typeof(T);

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>True if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		public bool Equals(ValueTypeDto<T> other) => m_hasValue && other.m_hasValue ? EqualityComparer<T>.Default.Equals(m_value, other.m_value) : m_hasValue == other.m_hasValue;

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns>True if the current object is equal to the <paramref name="obj"/> parameter; otherwise, false.</returns>
		public override bool Equals(object obj) => obj is ValueTypeDto<T> valueTypeDto && Equals(valueTypeDto);

		/// <summary>
		/// Retrieves the hash code of the object returned by the Value property.
		/// </summary>
		/// <returns>The hash code of the object returned by the Value property if the HasValue property is true; otherwise zero.</returns>
		public override int GetHashCode() => m_hasValue ? EqualityComparer<T>.Default.GetHashCode(m_value) : 0;

		/// <summary>
		/// Retrieves the value of the current ValueTypeDto{T} object, or the object's default value.
		/// </summary>
		/// <returns>The value of the Value property if the HasValue property is true; otherwise, the default value of the type T.</returns>
		public T GetValueOrDefault() => GetValueOrDefault(default);

		/// <summary>
		/// Retrieves the value of the current ValueTypeDto{T} object, or the specified default value.
		/// </summary>
		/// <param name="defaultValue">A value to return if the HasValue property is false.</param>
		/// <returns>The value of the Value property if the HasValue property is true; otherwise, the defaultValue parameter.</returns>
		public T GetValueOrDefault(T defaultValue) => m_hasValue ? m_value : defaultValue;

		/// <summary>
		/// Returns the text representation of the value of the current ValueTypeDto{T} object.
		/// </summary>
		/// <returns>The text representation of the object returned by the Value property if the HasValue property is true and the Value is not null; otherwise the empty string.</returns>
		public override string ToString() => m_hasValue ? m_value.ToString() : "";

		/// <summary>
		/// Creates a new ValueTypeDto{T} object initialized to a specified value. 
		/// </summary>
		/// <param name="value">A value type.</param>
		/// <returns>An ValueTypeDto{T} object whose Value property is initialized with the value parameter.</returns>
		public static implicit operator ValueTypeDto<T>(T value) => new ValueTypeDto<T>(value);

		/// <summary>
		/// Returns the value of a specified ValueTypeDto{T} value.
		/// </summary>
		/// <param name="valueTypeDto">An ValueTypeDto{T} value.</param>
		/// <returns>The value of the Value property for the value parameter.</returns>
		/// <exception cref="InvalidOperationException">The HasValue property is false.</exception>
		public static implicit operator T(ValueTypeDto<T> valueTypeDto) => valueTypeDto.Value;

		/// <summary>
		/// Compares two instances for equality.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns><c>true</c> the instances are equal; otherwise, <c>false</c>.</returns>
		public static bool operator ==(ValueTypeDto<T> left, ValueTypeDto<T> right) => left.Equals(right);

		/// <summary>
		/// Compares two instances for inequality.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
		public static bool operator !=(ValueTypeDto<T> left, ValueTypeDto<T> right) => !left.Equals(right);

		readonly bool m_hasValue;
		readonly T m_value;
	}
}
