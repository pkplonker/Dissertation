using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(MeshJsonConverter))]
	public class MeshJsonConverter : JsonConverter<UnityEngine.Mesh>
	{
		public override void WriteJson(JsonWriter writer, Mesh value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();

			writer.WritePropertyName("vertices");
			serializer.Serialize(writer, value.vertices);
			writer.WritePropertyName("triangles");
			serializer.Serialize(writer, value.triangles);
			writer.WritePropertyName("bounds");
			serializer.Serialize(writer, value.bounds);
			writer.WritePropertyName("colors");
			serializer.Serialize(writer, value.colors);
			writer.WritePropertyName("colors32");
			serializer.Serialize(writer, value.colors32);
			writer.WritePropertyName("normals");
			serializer.Serialize(writer, value.normals);
			writer.WritePropertyName("tangents");
			serializer.Serialize(writer, value.tangents);
			writer.WritePropertyName("uv");
			serializer.Serialize(writer, value.uv);
			writer.WritePropertyName("uv2");
			serializer.Serialize(writer, value.uv2);
			writer.WritePropertyName("uv3");
			serializer.Serialize(writer, value.uv3);
			writer.WritePropertyName("uv4");
			serializer.Serialize(writer, value.uv4);
			writer.WritePropertyName("uv5");
			serializer.Serialize(writer, value.uv5);
			writer.WritePropertyName("uv6");
			serializer.Serialize(writer, value.uv6);
			writer.WritePropertyName("uv7");
			serializer.Serialize(writer, value.uv7);
			writer.WritePropertyName("uv8");
			serializer.Serialize(writer, value.uv8);
			writer.WritePropertyName("name");
			writer.WriteValue(value.name);
			writer.WriteEndObject();
		}

		public override Mesh ReadJson(JsonReader reader, Type objectType, Mesh existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new JsonSerializationException(
					$"Unexpected token {reader.TokenType} when deserializing GameObject.");
			}

			var obj = JObject.Load(reader);

			var mesh = new Mesh();

			if (obj["vertices"] != null)
			{
				mesh.vertices = obj["vertices"].ToObject<Vector3[]>();
			}

			if (obj["triangles"] != null)
			{
				mesh.triangles = obj["triangles"].ToObject<int[]>();
			}

			if (obj["bounds"] != null)
			{
				mesh.bounds = obj["bounds"].ToObject<Bounds>();
			}

			if (obj["colors"] != null)
			{
				mesh.colors = obj["colors"].ToObject<Color[]>();
			}

			if (obj["colors32"] != null)
			{
				mesh.colors32 = obj["colors32"].ToObject<Color32[]>();
			}

			if (obj["normals"] != null)
			{
				mesh.normals = obj["normals"].ToObject<Vector3[]>();
			}

			if (obj["tangents"] != null)
			{
				mesh.tangents = obj["tangents"].ToObject<Vector4[]>();
			}

			if (obj["uv"] != null)
			{
				mesh.uv = obj["uv"].ToObject<Vector2[]>();
			}

			if (obj["uv2"] != null)
			{
				mesh.uv2 = obj["uv2"].ToObject<Vector2[]>();
			}

			if (obj["uv3"] != null)
			{
				mesh.uv3 = obj["uv3"].ToObject<Vector2[]>();
			}

			if (obj["uv4"] != null)
			{
				mesh.uv4 = obj["uv4"].ToObject<Vector2[]>();
			}

			if (obj["uv5"] != null)
			{
				mesh.uv5 = obj["uv5"].ToObject<Vector2[]>();
			}

			if (obj["uv6"] != null)
			{
				mesh.uv6 = obj["uv6"].ToObject<Vector2[]>();
			}

			if (obj["uv7"] != null)
			{
				mesh.uv7 = obj["uv7"].ToObject<Vector2[]>();
			}

			if (obj["uv8"] != null)
			{
				mesh.uv8 = obj["uv8"].ToObject<Vector2[]>();
			}

			if (obj["name"] != null)
			{
				mesh.name = obj["name"].ToString();
			}

			return mesh;
		}
	}
}