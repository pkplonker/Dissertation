using System;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class PropertyChangeHandler : IRTUCommandHandler
	{
		public void Process(NetworkStream stream, string payload)
		{
			MainThreadDispatcher.Instance.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<PropertyChangeArgs>(payload);
					var go = GameObject.Find(args.GameObjectPath);
					Type type = Type.GetType(args.ComponentTypeName);
					var component = go.GetComponent(type);
					var propertySplit = args.PropertyPath.Split('.');
					string fieldName = propertySplit[0];
					string subFieldName = propertySplit[1];
					string value = args.Value;
					fieldName = fieldName.Trim("m_".ToCharArray());
					PropertyInfo[] props =
						type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					PropertyInfo propInfo = null;
					foreach (var prop in props)
					{
						if (string.Equals(prop.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
						{
							propInfo = prop;
							break;
						}
					}

					if (propInfo != null)
					{
						var propType = propInfo.PropertyType;

						if (propType.IsValueType)
						{
							var currentStructValue = propInfo.GetValue(component);
							var modifiedStruct = ModifyStruct(currentStructValue, subFieldName, value);
							propInfo.SetValue(component, modifiedStruct);
						}
						else
						{
							propInfo.SetValue(component, ConvertValue(propInfo.PropertyType, value));
						}

						Debug.Log($"{fieldName}.{subFieldName} set to {value} successfully.");
					}
				}
				catch (Exception e)
				{
					Debug.Log($"Failed to set property: {e.Message}");
				}
			});
		}

		private object ModifyStruct(object structValue, string subFieldName, string newValue)
		{
			Type structType = structValue.GetType();

			FieldInfo subFieldInfo = structType.GetField(subFieldName, BindingFlags.Public | BindingFlags.Instance);

			if (subFieldInfo != null)
			{
				object convertedValue = Convert.ChangeType(newValue, subFieldInfo.FieldType);

				object newStruct = structValue;

				subFieldInfo.SetValue(newStruct, convertedValue);

				return newStruct;
			}

			Debug.LogError($"Subfield '{subFieldName}' not found in struct '{structType.Name}'.");
			return structValue;
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