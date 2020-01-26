using System;
using System.Linq;
using System.Reflection;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Supports JSON conversion of Optional{T}.
	/// </summary>
	public class OptionalJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType.IsGenericType() && objectType.GetGenericTypeDefinition() == typeof(Optional<>);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// make sure it has a value; optional instances without a value must be ignored
			IOptional optional = (IOptional) value;
			if (!optional.HasValue)
			{
				string optionalValueTypeName = optional.GetType().GenericTypeArguments.Single().Name;
				throw new InvalidOperationException(("Optional<{0}>.HasValue is false. " +
					"Optional properties should include these attributes: " +
						"[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(Optional<{0}>))]")
							.FormatInvariant(optionalValueTypeName));
			}

			// serialize value
			serializer.Serialize(writer, optional.Value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// get T of Optional<T>
			Type optionalValueType = objectType.GenericTypeArguments.Single();

			// deserialize using T
			object optionalValue = serializer.Deserialize(reader, optionalValueType);

			// call Optional<T>(T value) constructor
			ConstructorInfo constructorInfo = GetConstructor(objectType, new[] { optionalValueType });
			Verify.IsNotNull(constructorInfo);
			return constructorInfo.Invoke(new[] { optionalValue });
		}

		private static ConstructorInfo GetConstructor(Type type, Type[] types) =>
			type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.IsPublic && EnumerableUtility.AreEqual(x.GetParameters().Select(p => p.ParameterType), types));
	}
}
