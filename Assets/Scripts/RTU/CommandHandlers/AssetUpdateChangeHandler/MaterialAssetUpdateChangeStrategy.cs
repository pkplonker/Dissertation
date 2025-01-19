using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class MaterialAssetUpdateChangeStrategy : BaseAssetUpdateChangeStrategy
	{
		public override string EXTENSION { get; } = "mat";

		public override void Update(string payload, JsonSerializerSettings jsonSettings)
		{
			base.Update(payload, jsonSettings);
			var args = JsonConvert.DeserializeObject<MaterialAssetPropertyChangeEventArgs>(payload,
				jsonSettings);

			var elements = GetElements();
			var assetName = Path.GetFileNameWithoutExtension(args.Path);
			var matchingAssets = elements.WhereNotNull().Where(x =>
			{
				var name = RemoveInstanceString(x.name, IAssetUpdateChangeStrategy.INSTANCE_STRING);

				return name.TrimEnd(' ').Equals(assetName, StringComparison.InvariantCultureIgnoreCase);
			}).OfType<Material>();

			if (matchingAssets.Any())
			{
				foreach (var mat in matchingAssets)
				{
					Perform(args.ShaderProperties, mat, args.Path);
				}
			}
		}

		public override void MultiUpdate(string payload, JsonSerializerSettings jsonSettings)
		{
			throw new NotImplementedException();
		}

		public static void Perform(Dictionary<string,object> changes, Material mat, string path)
		{
			foreach (var change in changes)
			{
				try
				{
					var value = change.Value;

					if (mat.HasFloat(change.Key))
					{
						var val = (float) Convert.ChangeType(value, typeof(float));
						mat.SetFloat(change.Key, val);
					}
					else if (mat.HasInt(change.Key))
					{
						mat.SetInteger(change.Key, (int) Convert.ChangeType(value, typeof(int)));
					}
					else if (mat.HasVector(change.Key))
					{
						mat.SetVector(change.Key, (Vector4) Convert.ChangeType(value, typeof(Vector4)));
					}

					RTUDebug.Log($"Updated asset {change.Key} on {path}");
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed setting {change.Key} on {path}: {e.Message}");
				}
			}
		}

		protected override List<UnityEngine.Object> GetElements()
		{
			var elements = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Select(x => x.TryGetComponent<Renderer>(out var comp) ? comp : null).WhereNotNull()
				.SelectMany(x => x.materials).Cast<UnityEngine.Object>().ToList();
			return elements;
		}
	}
}