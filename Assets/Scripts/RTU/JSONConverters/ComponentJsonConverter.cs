using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	[JSONCustomConverter(typeof(ComponentJsonConverter))]
	public class ComponentJsonConverter : JsonConverter<UnityEngine.Component>
	{
		public override void WriteJson(JsonWriter writer, Component value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("GameObjectPath");
			writer.WriteValue(value.gameObject.GetFullName());
			writer.WritePropertyName("ComponentType");
			writer.WriteValue(value.GetType().AssemblyQualifiedName);
			writer.WriteEndObject();
		}

		public override Component ReadJson(JsonReader reader, Type objectType, Component existingValue,
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

			string gameObjectPath = null;
			Type componentType = null;

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

					if (propertyName == "ComponentType")
					{
						reader.Read();
						componentType = Type.GetType((string) reader.Value);
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
				throw new JsonSerializationException($"GameObject with path {gameObjectPath} could not be found.");
			}

			var comp = gameObject.GetComponent(componentType);
			if (gameObject == null)
			{
				throw new JsonSerializationException(
					$"Component with path {gameObjectPath} - {componentType?.Name} could not be found.");
			}

			return comp;
		}
	}
}