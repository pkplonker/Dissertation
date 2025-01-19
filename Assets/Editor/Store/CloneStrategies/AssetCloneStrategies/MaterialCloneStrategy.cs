using System;
using RealTimeUpdateRuntime;
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

				foreach (var val in mat.GetPropertyNames(MaterialPropertyType.Float))
				{
					clone.ShaderProperties.Add(val, mat.GetFloat(val));
				}

				foreach (var val in mat.GetPropertyNames(MaterialPropertyType.Vector))
				{
					clone.ShaderProperties.Add(val,mat.GetVector(val));
				}

				foreach (var fl in mat.GetPropertyNames(MaterialPropertyType.Int))
				{
					clone.ShaderProperties.Add(fl, mat.GetInteger(fl));
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