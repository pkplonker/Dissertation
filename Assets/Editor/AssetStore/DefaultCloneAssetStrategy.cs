using System;
using RealTimeUpdateRuntime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class DefaultCloneAssetStrategy : ICloneAssetStrategy
	{
		public virtual Clone CloneAsset(Object asset) => CloneInternal(asset, asset.GetType(), new Clone());

		protected Clone CloneInternal(Object asset, Type type, Clone clone)
		{
			// Getting the member adaptors each asset is ineffective, but will ignore for the time being as it's not currently an issue,
			// and will require some thought as you could have derived types of assets
			foreach (var prop in MemberAdaptorUtils.GetMemberAdapters(type))
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
					Debug.LogWarning($"Failed to add {prop.Name} to clone dictionary of {asset.name}");
				}
			}

			return clone;
		}
	}
}