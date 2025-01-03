using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(AssetJsonConverter))]
	public class AssetJsonConverter : JsonConverter<Object>
	{
		private readonly Dictionary<Type, JsonConverter> converters;
		private readonly JsonConverter defaultComponentConverter;

		public AssetJsonConverter()
		{
			converters = TypeRepository.GetTypes()
				.Where(x =>
				{
					if (!x.IsSubclassOf(typeof(JsonConverter)) || (!(x.BaseType?.IsGenericType ?? false))) return false;
					var genericType = x.BaseType.GenericTypeArguments.FirstOrDefault();
					return genericType != null && genericType.IsSubclassOf(typeof(UnityEngine.Object));
				}).ToDictionary(x => x.BaseType.GenericTypeArguments.FirstOrDefault(),
					x => Activator.CreateInstance(x) as JsonConverter);
			defaultComponentConverter = converters[typeof(UnityEngine.Component)];
		}

		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var valueType = value.GetType();
			if (converters.TryGetValue(valueType, out var converter))
			{
				converter.WriteJson(writer, value, serializer);
			}
			else if (valueType.IsSubclassOf(typeof(Component)))
			{
				defaultComponentConverter.WriteJson(writer, value, serializer);
			}
			else
			{
				try
				{
					serializer.Serialize(writer, value);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to serialize type {valueType} in AssetJSONConverter {e.Message}");
				}
			}
		}

		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (converters.TryGetValue(objectType, out var converter))
			{
				return converter.ReadJson(reader, objectType, existingValue, serializer) as Object;
			}

			if (objectType.IsSubclassOf(typeof(Component)))
			{
				return defaultComponentConverter.ReadJson(reader, objectType, existingValue, serializer) as Object;
			}

			try
			{
				return serializer.Deserialize(reader, objectType) as Object;
			}
			catch (Exception e)
			{
				RTUDebug.LogWarning($"Failed to serialize type {objectType} in AssetJSONConverter {e.Message}");
				return null;
			}
		}
	}
}