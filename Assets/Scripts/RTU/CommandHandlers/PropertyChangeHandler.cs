using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class PropertyChangeHandler : RTUCommandHandlerBase
	{
		
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<PropertyChangeArgs>(commandHandlerArgs.Payload);
					var go = GameObject.Find(args.GameObjectPath);
					var type = Type.GetType(args.ComponentTypeName);
					if (type == null)
					{
						throw new Exception("Could not determine property update type");
					}

					var component = go.GetComponent(type);
					var propertySplit = args.PropertyPath.Split('.');
					var fieldName = propertySplit[0];
					var subFieldName = string.Empty;
					// todo will this need changing to support nested values?
					if (propertySplit.Length > 1)
					{
						subFieldName = propertySplit[1];
					}

					var value = args.GetDeserializedValue(jsonSettings);
					fieldName = fieldName.Trim("m_".ToCharArray());
					IMemberAdapter member = null;
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
						catch (Exception e)
						{
							throw;
						}
					}

					var memberType = member.MemberType;
					bool set = false;
					if (memberType.IsValueType)
					{
						var currentStructValue = member.GetValue(component);

						try
						{
							if (ModifyStruct(currentStructValue, subFieldName, value, out var modifiedStruct))
							{
								member.SetValue(component, modifiedStruct);
								set = true;
								RTUDebug.Log($"{fieldName}.{subFieldName} set to {value} successfully.");
							}
						}
						catch
						{
							// this is ok as it could not be a struct with values, but rather an intrinsic struct, such as float, int, bool
						}
					}

					if (!set)
					{
						var convertedVal = ConvertValue(memberType, value, jsonSettings);
						member.SetValue(component, convertedVal);
						RTUDebug.Log($"{fieldName} set to {convertedVal} successfully.");
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message}");
				}
			});
		}

		private bool ModifyStruct(object structValue, string subFieldName, object newValue, out object newStruct)
		{
			Type structType = structValue.GetType();

			FieldInfo subFieldInfo = structType.GetField(subFieldName, BindingFlags.Public | BindingFlags.Instance);
			newStruct = structType;
			if (subFieldInfo == null) return false;
			object convertedValue = Convert.ChangeType(newValue, subFieldInfo.FieldType);
			newStruct = structValue;
			subFieldInfo.SetValue(newStruct, convertedValue);
			return true;
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
				if (value is string strValue)
				{
					return strValue == "1" || strValue.Equals("true", StringComparison.OrdinalIgnoreCase);
				}

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