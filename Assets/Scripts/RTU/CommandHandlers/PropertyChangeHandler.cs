using System;
using System.Collections;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public class PropertyChangeHandler : PropertyRTUCommandHandlerBase
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<PropertyChangeArgs>(commandHandlerArgs, out var component,
						out var fieldName, out var member) as PropertyChangeArgs;
					var value = args.GetDeserializedValue(jsonSettings);
					var memberType = member.MemberType;
					var convertedVal = ConvertValue(memberType, value, jsonSettings);
					member.SetValue(component, convertedVal);
					RTUDebug.Log($"{fieldName} set to {convertedVal} successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message}");
				}
			});
		}
		
		private static object ConvertValue(Type targetType, object value, JsonSerializerSettings jsonSettings)
		{
			if (value == null || targetType == null)
				return null;
			if (targetType.IsArray) return value;
			if (value is IEnumerable) return value;
			if (value.GetType() == targetType) return value;
			if (targetType == typeof(bool))
			{
				return Convert.ToBoolean(value);
			}

			if (targetType.IsEnum)
			{
				return Enum.Parse(targetType, value.ToString());
			}

			if (targetType != typeof(string) && targetType.IsClass)
			{
				return JsonConvert.DeserializeObject(value.ToString(), targetType, jsonSettings);
			}

			if (targetType == typeof(int))
			{
				if (value is IConvertible)
				{
					long longValue = Convert.ToInt64(value);
					if (longValue == 4294967295)
					{
						return -1;
					}
				}
			}

			return Convert.ChangeType(value, targetType);
		}
	}
}