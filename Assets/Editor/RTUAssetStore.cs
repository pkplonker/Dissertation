using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Clone : Dictionary<string, object> { }

public class AssetTypeCollection : Dictionary<string, Clone> { }

namespace RTUEditor
{
	[InitializeOnLoad]
	public static class RTUAssetStore
	{
		private static Dictionary<string, AssetTypeCollection> assets;
		private static HashSet<string> excludedPaths = new() {"Packages"};

		private static HashSet<string> assetTypes = new()
		{
			"Shader",
			"Mat"
		};

		static RTUAssetStore()
		{
			assets = GenerateDictionary();
		}

		public static Dictionary<string, AssetTypeCollection> GenerateDictionary()
		{
			var assetPaths = AssetDatabase.GetAllAssetPaths()
				.Where(x => !excludedPaths.Any(excluded => x.StartsWith(excluded, StringComparison.InvariantCultureIgnoreCase)))
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
					var assetClone = CloneAsset(asset);
					if (!assetDict[assetType].TryAdd(path, assetClone))
					{
						Debug.LogWarning($"Failed to add {path} as element already exists");
					}
				}
			}

			return assetDict;
		}

		private static Clone CloneAsset(Object asset)
		{
			var type = asset.GetType();
			var clone = new Clone();

			return clone;
		}

		public static Dictionary<string, object> GetChangedProperties(object changeObject, string objectPath)
		{
			return null;
		}
	}
}