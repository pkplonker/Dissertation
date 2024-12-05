using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RealTimeUpdateRuntime;
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

		public static Dictionary<string, object> GetChangedProperties(object changeObject, string objectPath)
		{
			return null;
		}
	}
}