using System;
using System.Linq;
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
					var type = Type.GetType(args.ComponentTypeName);
					if (type == null)
					{
						throw new Exception("Could not determine property update type");
					}

					var component = go.GetComponent(type);
					var propertySplit = args.PropertyPath.Split('.');
					var fieldName = propertySplit[0];
					var subFieldName = string.Empty;
					if (propertySplit.Length > 1)
					{
						subFieldName = propertySplit[1];
					}

					var value = args.Value;
					fieldName = fieldName.Trim("m_".ToCharArray());
					MemberInfo[] members =
						type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							.Concat(type
								.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
								.OfType<MemberInfo>()).ToArray();
					IMemberAdapter member = CreateMemberAdapter(members.First(x =>
						string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase)));

					var memberType = member.MemberType;

					if (memberType.IsValueType)
					{
						var currentStructValue = member.GetValue(component);
						var modifiedStruct = ModifyStruct(currentStructValue, subFieldName, value);
						member.SetValue(component, modifiedStruct);
					}
					else
					{
						member.SetValue(component, ConvertValue(memberType, value));
					}

					Debug.Log($"{fieldName}.{subFieldName} set to {value} successfully.");
				}
				catch (Exception e)
				{
					Debug.Log($"Failed to set property: {e.Message}");
				}
			});
		}

		private static IMemberAdapter CreateMemberAdapter(MemberInfo memberInfo)
		{
			return memberInfo switch
			{
				PropertyInfo prop => new PropertyAdapter(prop),
				FieldInfo field => new FieldAdapter(field),
				_ => throw new InvalidOperationException("Unsupported member type.")
			};
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