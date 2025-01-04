using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class GameObjectPropertyChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = GameObjectPropertyChangeArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal(commandHandlerArgs, out var fieldName, out var member);
					var value = args.GetDeserializedValue(jsonSettings);
					var memberType = member.MemberType;
					var convertedVal = ConvertValue(memberType, value, jsonSettings);
					var go = GameObject.Find(args.GameObjectPath);
					member.SetValue(go, convertedVal);
					RTUDebug.Log($"{fieldName} set to {convertedVal} successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set GameObject property: {e.Message} : {e?.InnerException}");
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

		public GameObjectPropertyChangeArgs ProcessInternal(CommandHandlerArgs commandHandlerArgs,
			out string fieldName, out IMemberAdapter member)
		{
			var args = JsonConvert.DeserializeObject<GameObjectPropertyChangeArgs>(commandHandlerArgs.Payload);
			var go = GameObject.Find(args.GameObjectPath);

			fieldName = args.PropertyPath;

			fieldName = fieldName.Trim("m_".ToCharArray());
			member = null;
			try
			{
				member = MemberAdaptorUtils.GetMemberAdapter(typeof(GameObject), fieldName);
			}
			catch
			{
				try
				{
					member = MemberAdaptorUtils.GetMemberAdapter(typeof(GameObject),
						new string(fieldName.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()));
				}
				catch (Exception e) { }
			}

			return args;
		}
	}
}