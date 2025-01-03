using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(BoxColliderJsonConverter))]
	public class PhysicsMaterialJsonConverter : JsonConverter<PhysicMaterial>
	{
		public override void WriteJson(JsonWriter writer, PhysicMaterial value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();

			writer.WritePropertyName("name");
			writer.WriteValue(value.name);
			writer.WritePropertyName("dynamicFriction");
			writer.WriteValue(value.dynamicFriction);
			writer.WritePropertyName("staticFriction");
			writer.WriteValue(value.staticFriction);
			writer.WritePropertyName("bounciness");
			writer.WriteValue(value.bounciness);
			writer.WritePropertyName("frictionCombine");
			writer.WriteValue(value.frictionCombine.ToString());
			writer.WritePropertyName("bounceCombine");
			writer.WriteValue(value.bounceCombine.ToString());
			writer.WriteEndObject();
		}

		public override PhysicMaterial ReadJson(JsonReader reader, Type objectType, PhysicMaterial existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var obj = JObject.Load(reader);

			var material = new PhysicMaterial(obj["name"]?.ToString());
			material.dynamicFriction = obj["dynamicFriction"]?.ToObject<float>() ?? 0;
			material.staticFriction = obj["staticFriction"]?.ToObject<float>() ?? 0;
			material.bounciness = obj["bounciness"]?.ToObject<float>() ?? 0;

			if (obj["frictionCombine"] != null &&
			    Enum.TryParse<PhysicMaterialCombine>(obj["frictionCombine"].ToString(), out var frictionCombine))
			{
				material.frictionCombine = frictionCombine;
			}

			if (obj["bounceCombine"] != null &&
			    Enum.TryParse<PhysicMaterialCombine>(obj["bounceCombine"].ToString(), out var bounceCombine))
			{
				material.bounceCombine = bounceCombine;
			}

			return material;
		}
	}
}