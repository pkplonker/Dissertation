using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class PropertyChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = ComponentPropertyPayload.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<ComponentPropertyPayload>(commandHandlerArgs, out var component,
						out var fieldName, out var member) as ComponentPropertyPayload;
					var value = args.GetDeserializedValue(jsonSettings);
					var memberType = member.MemberType;
					var convertedVal = ConvertValue(memberType, value, jsonSettings);
					member.SetValue(component, convertedVal);
					RTUDebug.Log($"{fieldName} set to {convertedVal} successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message} : {e?.InnerException}");
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

		public IPropertyPayload ProcessInternal<T>(CommandHandlerArgs commandHandlerArgs,
			out Component component, out string fieldName, out IMemberAdapter member) where T : IPropertyPayload
		{
			var args = JsonConvert.DeserializeObject<T>(commandHandlerArgs.Payload);
			var go = GameObject.Find(args.GameObjectPath);
			var type = Type.GetType(args.ComponentTypeName);
			if (type == null)
			{
				throw new Exception("Could not determine property update type");
			}

			component = go.GetComponent(type);
			fieldName = args.PropertyPath;

			fieldName = fieldName.Trim("m_".ToCharArray());
			member = null;
			try
			{
				member = MemberAdaptorUtils.GetMemberAdapter(type, fieldName);
			}
			catch
			{
				try
				{
					member = MemberAdaptorUtils.GetMemberAdapter(type,
						new string(fieldName.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()));
				}
				catch (Exception e) { }
			}

			return args;
		}
	}
}