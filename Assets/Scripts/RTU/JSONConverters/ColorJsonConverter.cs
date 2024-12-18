using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	[JSONCustomConverter(typeof(ColorJsonConverter))]
	public class ColorJsonConverter : JsonConverter<UnityEngine.Color>
	{
		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("r");
			writer.WriteValue(value.r);
			writer.WritePropertyName("g");
			writer.WriteValue(value.g);
			writer.WritePropertyName("b");
			writer.WriteValue(value.b);
			writer.WritePropertyName("a");
			writer.WriteValue(value.a);
			writer.WriteEndObject();
		}

		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return Color.clear;
			}

			var color = new Color();
			reader.Read();

			while (reader.TokenType == JsonToken.PropertyName)
			{
				if (reader.Value != null)
				{
					var propertyName = reader.Value.ToString();
					reader.Read();

					switch (propertyName)
					{
						case "r":
							color.r = Convert.ToSingle(reader.Value);
							break;
						case "g":
							color.g = Convert.ToSingle(reader.Value);
							break;
						case "b":
							color.b = Convert.ToSingle(reader.Value);
							break;
						case "a":
							color.a = Convert.ToSingle(reader.Value);
							break;
					}
				}

				reader.Read();
			}

			return color;
		}
	}
}