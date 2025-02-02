using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class DefaultAssetChangePayloadStrategy : IAssetChangePayloadStrategy
	{
		public virtual bool TryGenerateArgs(Clone existingClone, Clone currentClone, UnityEngine.Object asset,
			out AssetPropertyChangeEventArgs args)
		{
			if (HasChange(currentClone, existingClone, asset,out var changes, out var originalValues))
			{
				UpdateAssetStoreWithLatest(currentClone);
				args = CreateArgs(currentClone, changes, asset, originalValues);
				return true;
			}

			args = null;
			return false;
		}

		protected void UpdateAssetStoreWithLatest(Clone currentClone)
		{
			RTUAssetStore.UpdateClone(currentClone);
		}

		protected virtual AssetPropertyChangeEventArgs CreateArgs(Clone currentClone,
			Dictionary<string, object> changes, UnityEngine.Object asset, Dictionary<string, object> originalValues)
		{
			var args = new AssetPropertyChangeEventArgs
			{
				ID = asset.GetInstanceID(),
				Path = currentClone.Name,
				Changes = changes,
				OriginalValues = originalValues,
				Type = currentClone.Type
			};
			return args;
		}

		protected virtual bool HasChange(Clone existingClone, Clone currentClone, Object asset,
			out Dictionary<string, object> changes, out Dictionary<string, object> originalValues)
		{
			// working on the assumption that only the values change.
			var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(asset.GetType());
			changes = existingClone.Except(currentClone).ToDictionary(x => x.Key, x => adaptors[x.Key].GetValue(asset));
			originalValues = currentClone.Except(existingClone).ToDictionary(x => x.Key, x => adaptors[x.Key].GetValue(asset));

			return changes.Any();
		}
	}
}