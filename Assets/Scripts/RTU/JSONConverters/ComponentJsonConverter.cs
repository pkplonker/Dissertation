using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	[JSONCustomConverter(typeof(GameObjectJsonConverter))]
	public class GameObjectJsonConverter : JsonConverter<UnityEngine.GameObject>
	{
		public override void WriteJson(JsonWriter writer, GameObject value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("GameObjectPath");
			writer.WriteValue(value.gameObject.GetFullName());
			writer.WritePropertyName("ComponentType");
			writer.WriteValue(value.GetType().Name);
			writer.WriteEndObject();
		}

		public override GameObject ReadJson(JsonReader reader, Type objectType, GameObject existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			throw new NotImplementedException("Deserialization is not implemented yet.");
		}
	}
}