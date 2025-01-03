using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
	[JSONCustomConverter(typeof(Texture2DJsonConverter))]
	public class Texture2DJsonConverter : JsonConverter<UnityEngine.Texture2D>
	{
		public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
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
			textureObj["format"] = value.format.ToString();
			textureObj["mipmapCount"] = value.mipmapCount;

			textureObj.WriteTo(writer);
#endif
		}

		public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue,
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

			return texture;
		}
	}
}