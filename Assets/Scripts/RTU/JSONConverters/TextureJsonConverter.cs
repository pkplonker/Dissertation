using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(TextureJsonConverter))]
	public class TextureJsonConverter : JsonConverter<UnityEngine.Texture>
	{
		public override void WriteJson(JsonWriter writer, Texture value, JsonSerializer serializer)
		{
#if UNITY_EDITOR
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			JObject textureObj = new JObject
			{
				["name"] = value.name,
				["filterMode"] = value.filterMode.ToString(),
				["wrapMode"] = value.wrapMode.ToString(),
				["anisoLevel"] = value.anisoLevel
			};

			if (!value.isReadable)
			{
				string assetPath = AssetDatabase.GetAssetPath(value);
				TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

				if (importer != null && !importer.isReadable)
				{
					importer.isReadable = true;
					importer.SaveAndReimport();
				}
			}

			var decompressed = value.Decompress();
			byte[] textureBytes = decompressed.EncodeToPNG();
			string base64Data = Convert.ToBase64String(textureBytes);

			textureObj["imageData"] = base64Data;
			textureObj["width"] = value.width;
			textureObj["height"] = value.height;
			if (value is Texture2D texture2D)
			{
				textureObj["format"] = texture2D.format.ToString();
			}

			textureObj["mipmapCount"] = value.mipmapCount;
			textureObj["name"] = value.name;

			textureObj.WriteTo(writer);
#endif
		}

		public override Texture ReadJson(JsonReader reader, Type objectType, Texture existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;

			JObject textureObj = JObject.Load(reader);

			if (!textureObj.ContainsKey("imageData")) return null;

			string base64Data = textureObj["imageData"].ToString();
			byte[] textureBytes = Convert.FromBase64String(base64Data);

			Texture2D texture = new Texture2D(0, 0);
			texture.LoadImage(textureBytes);

			if (textureObj.ContainsKey("filterMode") &&
			    Enum.TryParse(textureObj["filterMode"].ToString(), out FilterMode filterMode))
			{
				texture.filterMode = filterMode;
			}

			if (textureObj.ContainsKey("wrapMode") &&
			    Enum.TryParse(textureObj["wrapMode"].ToString(), out TextureWrapMode wrapMode))
			{
				texture.wrapMode = wrapMode;
			}

			if (textureObj.ContainsKey("anisoLevel"))
			{
				texture.anisoLevel = (int) textureObj["anisoLevel"];
			}

			if (textureObj.ContainsKey("name"))
			{
				texture.name = (string) textureObj["name"];
			}

			texture.Apply();
			return texture;
		}
	}
}