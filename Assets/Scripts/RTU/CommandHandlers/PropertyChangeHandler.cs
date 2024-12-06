﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class PropertyChangeHandler : RTUCommandHandlerBase
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs)
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

					var value = args.Value;
					fieldName = fieldName.Trim("m_".ToCharArray());
					var member = MemberAdaptorUtils.GetMemberAdapter(type, fieldName);
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
								Debug.Log($"{fieldName}.{subFieldName} set to {value} successfully.");
							}
						}
						catch
						{
							// this is ok as it could not be a struct with values, but rather an intrinsic struct, such as float, int, bool
						}
					}

					if (!set)
					{
						member.SetValue(component, ConvertValue(memberType, value));
						Debug.Log($"{fieldName} set to {value} successfully.");
					}
				}
				catch (Exception e)
				{
					Debug.Log($"Failed to set property: {e.Message}");
				}
			});
		}



		private bool ModifyStruct(object structValue, string subFieldName, string newValue, out object newStruct)
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

		private static object ConvertValue(Type targetType, object value)
		{
			try
			{
				return Convert.ChangeType(value, targetType);
			}
			catch (InvalidCastException)
			{
				throw new InvalidCastException(
					$"Cannot convert value '{value}' of type {targetType} to {targetType}.");
			}
			catch (FormatException)
			{
				throw new FormatException($"Invalid format for value '{value}' when converting to {targetType}.");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to convert value '{value}' to {targetType}: {ex.Message}");
			}
		}
	}
}