using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(BoxColliderJsonConverter))]
	public class BoxColliderJsonConverter : JsonConverter<BoxCollider>
	{
		public override void WriteJson(JsonWriter writer, BoxCollider value, JsonSerializer serializer)
		{
			if (value is BoxCollider boxCollider)
			{
				writer.WriteStartObject();

				writer.WritePropertyName("center");
				serializer.Serialize(writer, boxCollider.center);

				writer.WritePropertyName("size");
				serializer.Serialize(writer, boxCollider.size);

				writer.WriteEndObject();
			}
		}

		public override BoxCollider ReadJson(JsonReader reader, Type objectType, BoxCollider existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			var collider = new GameObject().AddComponent<BoxCollider>();
			collider.center = obj["center"].ToObject<Vector3>();
			collider.size = obj["size"].ToObject<Vector3>();
			return collider;
		}
	}
}