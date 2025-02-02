using System;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
				["anisoLevel"] = value.anisoLevel,
			};

			if (value is Texture2D tex)
			{
				textureObj.Add("textureFormat", (int) tex.format);
				Color32[] data = tex.GetPixels32();
				var byteArray = MemoryMarshal.Cast<Color32, byte>(data).ToArray();
				var compressed = byteArray.Compress();
				textureObj["imageData"] = compressed;
			}

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

			textureObj["width"] = value.width;
			textureObj["height"] = value.height;
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

			Texture2D texture = new Texture2D((int) textureObj["width"], (int) textureObj["height"],
				TextureFormat.RGBA32,
				false);

			var rawTextureData = (byte[]) textureObj["imageData"];
			var decompressed = rawTextureData.Decompress();
			var colors = MemoryMarshal.Cast<byte, Color32>(decompressed).ToArray();
			texture.SetPixels32(colors);

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