using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.AssetStore
{
	[InitializeOnLoad]
	public static class RTUAssetStore
	{
		private static Dictionary<string, AssetTypeCollection> assets;
		private static HashSet<string> excludedPaths = new(StringComparer.InvariantCultureIgnoreCase) {"Packages"};
		private static AssetCloneStrategyFactory assetCloneStrategyFactory = new AssetCloneStrategyFactory();

		public static HashSet<string> AssetTypes = new(StringComparer.InvariantCultureIgnoreCase)
		{
			//"Shader",
			"Mat",
			//"PNG"
		};

		static RTUAssetStore()
		{
			GenerateDictionary();
		}

		public static void GenerateDictionary()
		{
			var assetPaths = AssetDatabase.GetAllAssetPaths()
				.Where(x => !excludedPaths.Any(excluded =>
					x.StartsWith(excluded, StringComparison.InvariantCultureIgnoreCase)))
				.ToList();
			Dictionary<string, AssetTypeCollection> assetDict =
				new Dictionary<string, AssetTypeCollection>(StringComparer.InvariantCultureIgnoreCase);

			foreach (var assetType in AssetTypes)
			{
				assetDict[assetType] = new AssetTypeCollection();
				var paths = assetPaths.Where(x =>
					x.EndsWith(assetType, StringComparison.InvariantCultureIgnoreCase));
				if (!paths.Any())
				{
					Debug.LogWarning($"Failed to locate any assets of type {assetType}");
				}

				foreach (var path in paths)
				{
					if (!GenerateClone(path, assetType, out var assetClone))
					{
						Debug.LogWarning($"Failed to generate clone for {path}");
					}

					if (!assetDict[assetType].TryAdd(path, assetClone))
					{
						Debug.LogWarning($"Failed to add {path} as element already exists");
					}
				}
			}

			assets = assetDict;
		}

		public static bool GenerateClone(string path, string assetType, out Clone assetClone)
		{
			assetClone = null;

			if (!AssetTypes.TryGetValue(assetType, out var _))
			{
				Debug.LogWarning($"Type {assetType} not supported in RTUAssetStore");
				return false;
			}

			var asset = AssetDatabase.LoadMainAssetAtPath(path);
			var mat = asset as Material;
			if (asset == null) return false;
			var strategy = assetCloneStrategyFactory.GetCloneStrategy(assetType);
			assetClone = strategy.Clone(asset, path);
			return true;
		}

		public static bool TryGetExistingClone(string changeAssetPath, string type, out Clone asset)
		{
			if (assets.TryGetValue(type, out var matchingAssets))
			{
				if (matchingAssets.TryGetValue(changeAssetPath, out var clone))
				{
					asset = clone;
					return true;
				}
			}

			asset = null;
			return false;
		}

		public static void UpdateClone(Clone newClone)
		{
			if (newClone == null) return;
			if (assets.TryGetValue(newClone.Type, out var matchingAssets))
			{
				if (matchingAssets.TryGetValue(newClone.Name, out var _))
				{
					matchingAssets[newClone.Name] = newClone;
				}
			}
		}
	}
}