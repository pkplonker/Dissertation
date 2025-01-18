using System;
using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEngine;
using Object = System.Object;

namespace RTUEditor.ObjectChange
{
	public class MaterialAssetChangePayloadStrategy : DefaultAssetChangePayloadStrategy
	{
		public override bool TryGenerateArgs(Clone originalClone, Clone currentClone, UnityEngine.Object asset,
			out AssetPropertyChangeEventArgs args)
		{
			if (base.TryGenerateArgs(originalClone, currentClone, asset, out args))
			{
				if (HasShaderChanges(originalClone as MaterialClone, currentClone as MaterialClone, out var changes,
					    out var originalValues))
				{
					var matArgs = new MaterialAssetPropertyChangeEventArgs(args)
					{
						ShaderProperties = changes,
						ShaderPropertiesOriginalValues = originalValues,
					};
					args = matArgs;
					UpdateAssetStoreWithLatest(currentClone);

					return true;
				}

				UpdateAssetStoreWithLatest(currentClone);

				return true;
			}
			else if (HasShaderChanges(originalClone as MaterialClone, currentClone as MaterialClone, out var changes,
				         out var originalValues))
			{
				var matArgs = CreateMaterialArgs(currentClone,
					new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), asset,
					new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase));
				args = matArgs;
				matArgs.ShaderProperties = changes;
				matArgs.ShaderPropertiesOriginalValues = originalValues;
				UpdateAssetStoreWithLatest(currentClone);

				return true;
			}

			return false;
		}

		private bool HasShaderChanges(MaterialClone existingClone, MaterialClone currentClone,
			out Dictionary<string, Dictionary<string, object>> changes,
			out Dictionary<string, Dictionary<string, object>> originalValues)
		{
			changes = new Dictionary<string, Dictionary<string, object>>();
			originalValues = new Dictionary<string, Dictionary<string, object>>();
			for (var i = 0; i < existingClone.ShaderProperties.Count; i++)
			{
				var key = existingClone.ShaderProperties.ElementAt(i).Key;
				originalValues.TryAdd(key, existingClone.ShaderProperties.ElementAt(i).Value
					.Except(currentClone.ShaderProperties.ElementAt(i).Value)
					.ToDictionary(x => x.Key, x => x.Value));
				changes.TryAdd(key, currentClone.ShaderProperties.ElementAt(i).Value
					.Except(existingClone.ShaderProperties.ElementAt(i).Value)
					.ToDictionary(x => x.Key, x => x.Value));
			}

			return changes.Any(x => x.Value.Any());
		}

		private MaterialAssetPropertyChangeEventArgs CreateMaterialArgs(Clone currentClone,
			Dictionary<string, object> changes, UnityEngine.Object asset, Dictionary<string, object> originalValues)
		{
			var args = new MaterialAssetPropertyChangeEventArgs
			{
				ID = asset.GetInstanceID(),
				Path = currentClone.Name,
				Changes = changes,
				OriginalValues = originalValues,
				Type = currentClone.Type
			};
			return args;
		}
	}
}