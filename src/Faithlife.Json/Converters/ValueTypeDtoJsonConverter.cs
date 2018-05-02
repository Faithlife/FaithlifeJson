using System;
using System.Linq;
using System.Reflection;
using Faithlife.Utility;
using Newtonsoft.Json;

namespace Faithlife.Json.Converters
{
	/// <summary>
	/// Supports JSON conversion of ValueTypeDto{T}.
	/// </summary>
	public class ValueTypeDtoJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType() && objectType.GetGenericTypeDefinition() == typeof(ValueTypeDto<>);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// make sure it has a value; instances without a value must be ignored
			var valueTypeDto = (IValueTypeDto) value;
			if (!valueTypeDto.HasValue)
			{
				string valueTypeName = valueTypeDto.GetType().GenericTypeArguments.Single().Name;
				throw new InvalidOperationException(("ValueTypeDto<{0}>.HasValue is false. " +
					"ValueTypeDto properties should include these attributes: " +
						"[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Include), DefaultValueDefault(typeof(ValueTypeDto<{0}>))]")
							.FormatInvariant(valueTypeName));
			}

			// serialize value
			serializer.Serialize(writer, valueTypeDto.Value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// get T of ValueTypeDto<T>
			Type valueType = objectType.GenericTypeArguments.Single();

			// deserialize using T
			object value = serializer.Deserialize(reader, valueType);

			// call ValueTypeDto<T>(T value) constructor
			ConstructorInfo constructorInfo = GetConstructor(objectType, new[] { valueType });
			Verify.IsNotNull(constructorInfo);
			return constructorInfo.Invoke(new[] { value });
		}

		private static ConstructorInfo GetConstructor(Type type, Type[] types)
			=> type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.IsPublic && EnumerableUtility.AreEqual(x.GetParameters().Select(p => p.ParameterType), types));
	}
}
