using System;
using System.Collections.Generic;
using RealTimeUpdateRuntime;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class DefaultCloneStrategy : ICloneStrategy
	{
		private static Dictionary<Type, List<IMemberAdapter>> memberAdaptorCollection = new ();

		public virtual Clone Clone(Object asset, string path) => CloneInternal(asset, asset.GetType(),
			new Clone(path, StringComparer.InvariantCultureIgnoreCase));

		protected Clone CloneInternal(Object asset, Type type, Clone clone)
		{
			foreach (var prop in GetMemberAdapters(type))
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

				if (!clone.TryAdd(prop.Name, val))
				{
					//Debug.LogWarning($"Failed to add {prop.Name} to clone dictionary of {asset.name}");
				}
			}

			return clone;
		}

		private static List<IMemberAdapter> GetMemberAdapters(Type type)
		{
			if (memberAdaptorCollection.TryGetValue(type, out var collection))
			{
				return collection;
			}

			var newCollection = MemberAdaptorUtils.GetMemberAdapters(type);
			memberAdaptorCollection[type] = newCollection;
			return newCollection;
		}
	}
}