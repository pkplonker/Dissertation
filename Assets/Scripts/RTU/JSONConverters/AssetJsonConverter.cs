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
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("GameObjectPath");

			writer.WriteValue(value.GetFullName() ?? string.Empty);

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