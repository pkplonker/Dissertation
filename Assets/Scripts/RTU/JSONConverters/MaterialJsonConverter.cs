using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(MaterialJsonConverter))]
	public class MaterialJsonConverter : JsonConverter<UnityEngine.Material>
	{
		public override void WriteJson(JsonWriter writer, Material value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();

			writer.WritePropertyName("shader");
			writer.WriteValue(value.shader.name);

			writer.WritePropertyName("color");
			serializer.Serialize(writer, value.color);

			writer.WritePropertyName("mainTexture");
			if (value.mainTexture != null)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("name");
				writer.WriteValue(value.mainTexture.name);
				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNull();
			}

			writer.WritePropertyName("mainTextureOffset");
			serializer.Serialize(writer, value.mainTextureOffset);
			writer.WritePropertyName("mainTextureScale");
			serializer.Serialize(writer, value.mainTextureScale);

			writer.WritePropertyName("properties");
			writer.WriteStartObject();
			foreach (string propertyName in value.GetTexturePropertyNames())
			{
				var texture = value.GetTexture(propertyName);
				if (texture != null)
				{
					writer.WritePropertyName(propertyName);
					writer.WriteStartObject();
					writer.WritePropertyName("name");
					writer.WriteValue(texture.name);
					writer.WriteEndObject();
				}
			}

			writer.WriteEndObject();

			writer.WriteEndObject();
		}

		public override Material ReadJson(JsonReader reader, Type objectType, Material existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var obj = JObject.Load(reader);
			
			var shaderName = obj["shader"]?.ToString() ?? "Lit";
			var material = new Material(Shader.Find(shaderName));

			if (obj["color"] != null)
			{
				material.color = obj["color"].ToObject<Color>();
			}

			if (obj["mainTexture"] != null)
			{
				var textureObj = obj["mainTexture"] as JObject;
				if (textureObj != null && textureObj["name"] != null)
				{
					var textureName = textureObj["name"].ToString();
					// todo material.mainTexture = Resources.Load<Texture>(textureName);
				}
			}

			if (obj["mainTextureOffset"] != null)
			{
				material.mainTextureOffset = obj["mainTextureOffset"].ToObject<Vector2>();
			}

			if (obj["mainTextureScale"] != null)
			{
				material.mainTextureScale = obj["mainTextureScale"].ToObject<Vector2>();
			}

			if (obj["properties"] != null)
			{
				var properties = obj["properties"] as JObject;
				if (properties != null)
				{
					foreach (var property in properties.Properties())
					{
						if (property.Value is JObject textureObj && textureObj["name"] != null)
						{
							var textureName = textureObj["name"].ToString();
							var texture = Resources.Load<Texture>(textureName);
							if (texture != null)
							{
								material.SetTexture(property.Name, texture);
							}
						}
					}
				}
			}

			return material;
		}
	}
}