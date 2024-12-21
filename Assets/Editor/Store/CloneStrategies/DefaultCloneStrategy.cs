using System;
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
					var array = (Array) val;
					var clonedArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
					Array.Copy(array, clonedArray, array.Length);
					val = clonedArray;
				}

				if (!clone.TryAdd(prop.Name, val))
				{
					//Debug.LogWarning($"Failed to add {prop.Name} to clone dictionary of {asset.name}");
				}
			}

			return clone;
		}
	}
}