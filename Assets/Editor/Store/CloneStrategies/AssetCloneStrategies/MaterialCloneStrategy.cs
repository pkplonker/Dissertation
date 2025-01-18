using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class MaterialCloneStrategy : DefaultCloneStrategy
	{
		public override Clone Clone(Object asset, string path)
		{
			var clone = CloneInternal(asset, asset.GetType(), new MaterialClone(path)) as MaterialClone;

			try
			{
				var mat = asset as Material;

				foreach (var fl in mat.GetPropertyNames(MaterialPropertyType.Float))
				{
					clone.ShaderProperties["float"].Add(fl, mat.GetFloat(fl));
				}
				foreach (var fl in mat.GetPropertyNames(MaterialPropertyType.Vector))
				{
					clone.ShaderProperties["vector"].Add(fl, mat.GetVector(fl));
				}
				foreach (var fl in mat.GetPropertyNames(MaterialPropertyType.Int))
				{
					clone.ShaderProperties["int"].Add(fl, mat.GetInteger(fl));
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to generate texture byte hash {asset.name}: {e.Message}");
			}

			return clone;
		}
	}
}