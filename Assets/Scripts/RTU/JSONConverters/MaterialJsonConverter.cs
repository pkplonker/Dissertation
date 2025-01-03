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
				serializer.Serialize(writer, value.mainTexture);
			}
			else
			{
				writer.WriteNull();
			}

			writer.WritePropertyName("mainTextureOffset");
			serializer.Serialize(writer, value.mainTextureOffset);
			writer.WritePropertyName("mainTextureScale");
			serializer.Serialize(writer, value.mainTextureScale);

			{
				writer.WritePropertyName("properties");
				writer.WriteStartObject();
				foreach (string propertyName in value.GetTexturePropertyNames())
				{
					var texture = value.GetTexture(propertyName);
					if (texture != null)
					{
						writer.WritePropertyName(propertyName);
						serializer.Serialize(writer, texture);
					}
				}

				writer.WriteEndObject();
			}
			writer.WritePropertyName("shaderKeywords");
			serializer.Serialize(writer, value.shaderKeywords);

			writer.WritePropertyName("enabledKeywords");
			writer.WriteStartArray();
			foreach (string keyword in value.shaderKeywords)
			{
				writer.WriteValue(keyword);
			}

			writer.WriteEndArray();
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

			if (obj["shaderKeywords"] != null)
			{
				var shaderKeywords = obj["shaderKeywords"].ToObject<string[]>();
				if (shaderKeywords != null)
				{
					foreach (var keyword in shaderKeywords)
					{
						material.EnableKeyword(keyword);
					}
				}
			}

			var names = material.GetTexturePropertyNames();
			foreach (var property in obj.Properties())
			{
				if (property.Name == "mainTexture" || property.Name == "properties")
				{
					var textureObj = property.Value as JObject;

					if (textureObj != null)
					{
						var texture = serializer.Deserialize<Texture>(textureObj.CreateReader());
						if (texture != null)
						{
							if (texture is Texture2D texture2D)
							{
								if (property.Name == "mainTexture")
								{
									material.mainTexture = texture2D;
								}
								else
								{
									material.SetTexture(property.Name == "mainTexture" ? "_MainTex" : property.Name,
										texture2D);
								}
							}
							else
							{
								RTUDebug.LogWarning("Deserialized texture is not a Texture2D.");
							}
						}
					}
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

			return material;
		}
	}
}