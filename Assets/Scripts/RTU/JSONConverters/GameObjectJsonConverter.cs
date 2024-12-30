using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealTimeUpdateRuntime
{
	[Preserve]
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
			bool hasExistingValue, JsonSerializer serializer)
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

			string gameObjectPath = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.PropertyName)
				{
					string propertyName = (string) reader.Value;

					if (propertyName == "GameObjectPath")
					{
						reader.Read();
						gameObjectPath = (string) reader.Value;
					}
				}
				else if (reader.TokenType == JsonToken.EndObject)
				{
					break;
				}
			}

			if (string.IsNullOrEmpty(gameObjectPath))
			{
				throw new JsonSerializationException("GameObjectPath is missing or empty.");
			}

			GameObject gameObject = GameObject.Find(gameObjectPath);

			if (gameObject == null)
			{
				throw new JsonSerializationException($"GameObject with path '{gameObjectPath}' could not be found.");
			}

			return gameObject;
		}
	}
}