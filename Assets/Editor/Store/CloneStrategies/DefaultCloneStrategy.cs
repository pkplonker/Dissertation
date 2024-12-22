using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class DefaultCloneStrategy : ICloneStrategy
	{
		private static Dictionary<Type, List<IMemberAdapter>> memberAdaptorCollection = new();

		public virtual Clone Clone(Object asset, string path) => CloneInternal(asset, asset.GetType(),
			new Clone(path, StringComparer.InvariantCultureIgnoreCase));

		protected Clone CloneInternal(Object asset, Type type, Clone clone, List<string> excludedProperties = null)
		{
			var adaptors = MemberAdaptorUtils.GetMemberAdapters(type)
				.Where(x => !excludedProperties?.Any(e =>
					string.Equals(e, x.Name, StringComparison.InvariantCultureIgnoreCase)) ?? false);
			foreach (var prop in adaptors)
			{
				object val = null;
				if (prop.MemberType.IsSubclassOf(typeof(Object)))
				{
					val = HandleUnityObject(asset, prop);
				}
				else
				{
					try
					{
						val = prop.GetValue(asset);
					}
					catch { }

					if (val == null)
					{
						//Debug.LogWarning($"Failed to get value for {prop.Name} to clone dictionary of {asset.name}");
						continue;
					}

					if (val.GetType().IsArray)
					{
						val = HandleArray(val);
					}
					else if (val is IList list)
					{
						val = HandleList(val, list);
					}
					else if (val is IDictionary dictionary)
					{
						//ignore for now?
					}
					else if (val is IEnumerable enumerable && !(val is string))
					{
						val = HandleCollection(val, enumerable);
					}
					else if (val.GetType().IsClass && val.GetType() != typeof(string) && val is not Object)
					{
						val = HandleClass(val);
					}
				}

				if (!clone.TryAdd(prop.Name, val))
				{
					//Debug.LogWarning($"Failed to add {prop.Name} to clone dictionary of {asset.name}");
				}
			}

			return clone;
		}

		private static object HandleClass(object val)
		{
			try
			{
				val = val.GetStaticHashCode();
			}
			catch { }

			return val;
		}

		private static object HandleCollection(object val, IEnumerable enumerable)
		{
			try
			{
				var clonedEnumerable = (IEnumerable) Activator.CreateInstance(val.GetType());
				var addMethod = clonedEnumerable.GetType().GetMethod("Add");
				if (addMethod != null)
				{
					foreach (var item in enumerable)
					{
						addMethod.Invoke(clonedEnumerable, new[] {item});
					}
				}

				val = clonedEnumerable;
			}
			catch { }

			return val;
		}

		private static object HandleList(object val, IList list)
		{
			try
			{
				// Handle lists
				var clonedList = (IList) Activator.CreateInstance(val.GetType());
				foreach (var item in list)
				{
					clonedList.Add(item);
				}

				val = clonedList;
			}
			catch { }

			return val;
		}

		private static object HandleArray(object val)
		{
			var array = (Array) val;
			var clonedArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
			Array.Copy(array, clonedArray, array.Length);
			val = clonedArray;
			return val;
		}

		private static object HandleUnityObject(Object asset, IMemberAdapter prop)
		{
			object val;
			var unityObject = prop.GetValue(asset) as UnityEngine.Object;
			if (unityObject != null)
			{
				val = unityObject.GetInstanceID();
			}
			else
			{
				val = null;
			}

			return val;
		}
	}
}