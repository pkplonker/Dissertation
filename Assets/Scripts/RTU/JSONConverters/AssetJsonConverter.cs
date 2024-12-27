using System;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	[JSONCustomConverter(typeof(AssetJsonConverter))]
	public class AssetJsonConverter : JsonConverter<Object>
	{
		private readonly ComponentJsonConverter componentJsonConverter;
		private readonly GameObjectJsonConverter gameObjectJsonConverter;

		public AssetJsonConverter()
		{
			gameObjectJsonConverter = new GameObjectJsonConverter();
			componentJsonConverter = new ComponentJsonConverter();
		}

		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var valueType = value.GetType();
			if (valueType == typeof(GameObject))
			{
				gameObjectJsonConverter.WriteJson(writer, value, serializer);
			}
			else if (valueType == typeof(Component) || valueType.IsSubclassOf(typeof(Component)))
			{
				componentJsonConverter.WriteJson(writer, value, serializer);
			}
			else
			{
				writer.WriteStartObject();
				//todo 
				writer.WriteEndObject();
			}
		}

		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (objectType == typeof(GameObject))
			{
				return gameObjectJsonConverter.ReadJson(reader, objectType, existingValue as GameObject,
					hasExistingValue,
					serializer);
			}

			if (objectType == typeof(Component) || objectType.IsSubclassOf(typeof(Component)))
			{
				return componentJsonConverter.ReadJson(reader, objectType, existingValue as Component,
					hasExistingValue,
					serializer);
			}

			throw new JsonSerializationException("Not implemented ");
		}
	}
}