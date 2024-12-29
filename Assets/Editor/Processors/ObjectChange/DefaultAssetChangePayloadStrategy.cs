using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;

namespace RTUEditor.ObjectChange
{
	public class DefaultAssetChangePayloadStrategy : IAssetChangePayloadStrategy
	{
		public virtual bool TryGenerateArgs(Clone originalClone, Clone currentClone,
			out AssetPropertyChangeEventArgs args)
		{
			if (HasChange(originalClone, currentClone, out var changes))
			{
				UpdateAssetStoreWithLatest(currentClone);
				args = CreateArgs(currentClone,changes);
				return true;
			}

			args = null;
			return false;
		}

		private void UpdateAssetStoreWithLatest(Clone currentClone)
		{
			RTUAssetStore.UpdateClone(currentClone);
		}

		protected virtual AssetPropertyChangeEventArgs CreateArgs(Clone currentClone,
			Dictionary<string, object> changes)
		{
			var args = new AssetPropertyChangeEventArgs
			{
				Path = currentClone.Name,
				Changes = changes,
				Type = currentClone.Type
			};
			return args;
		}

		protected virtual bool HasChange(Clone originalClone, Clone currentClone,
			out Dictionary<string, object> changes)
		{
			// working on the assumption that only the values change.
			changes = originalClone.Except(currentClone).ToDictionary(x => x.Key, x => x.Value);
			return changes.Any();
		}
	}
}