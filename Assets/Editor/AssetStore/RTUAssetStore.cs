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
		private static HashSet<string> excludedPaths = new() {"Packages"};

		private static HashSet<string> assetTypes = new()
		{
			"Shader",
			"Mat",
			"PNG"
		};

		private static ICloneAssetStrategy defaultCloneStrategy = new DefaultCloneAssetStrategy();

		private static Dictionary<string, ICloneAssetStrategy> cloneStrategies = new()
		{
			{"PNG", new TextureCloneAssetStrategy()},
		};

		static RTUAssetStore()
		{
			assets = GenerateDictionary();
		}

		public static Dictionary<string, AssetTypeCollection> GenerateDictionary()
		{
			var assetPaths = AssetDatabase.GetAllAssetPaths()
				.Where(x => !excludedPaths.Any(excluded =>
					x.StartsWith(excluded, StringComparison.InvariantCultureIgnoreCase)))
				.ToList();
			Dictionary<string, AssetTypeCollection> assetDict =
				new Dictionary<string, AssetTypeCollection>();

			foreach (var assetType in assetTypes)
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
					var asset = AssetDatabase.LoadMainAssetAtPath(path);
					if (asset == null) continue;
					var strategy = GetCloneStrategy(assetType);
					var assetClone = strategy.CloneAsset(asset);
					if (!assetDict[assetType].TryAdd(path, assetClone))
					{
						Debug.LogWarning($"Failed to add {path} as element already exists");
					}
				}
			}

			return assetDict;
		}

		private static ICloneAssetStrategy GetCloneStrategy(string assetType) =>
			cloneStrategies.TryGetValue(assetType, out var strategy) ? strategy : defaultCloneStrategy;

		public static Clone GetExistingClone(string objectPath)
		{
			return null;
		}
	}
}