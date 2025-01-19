using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class
		MaterialSubAssetChangePayloadRecorderTypeChangeUI : SubAssetChangePayloadRecorderTypeChangeUI<
		MaterialAssetPropertyChangeEventArgs>
	{
		public MaterialSubAssetChangePayloadRecorderTypeChangeUI(JsonSerializerSettings jsonSettings) : base(
			jsonSettings) { }

		public override List<AssetPropertyChangeEventArgs> Extract(List<AssetPropertyChangeEventArgs> possiblePayloads)
		{
			var interimPayloads = possiblePayloads.OfType<MaterialAssetPropertyChangeEventArgs>().ToList();
			var result = possiblePayloads.Except(interimPayloads).ToList();
			var customPayloads = new List<MaterialAssetPropertyChangeEventArgs>();
			foreach (var r in interimPayloads
				         .GroupBy(x => x.ID))
			{
				var changesDict = r
					.SelectMany(x => x.Changes)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.Last());
				var originalValues = r
					.SelectMany(x => x.OriginalValues)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.First());

				var shaderChangesDict = r
					.SelectMany(x => (x as MaterialAssetPropertyChangeEventArgs).ShaderProperties)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.Last());
				var ShaderOriginalValues = r
					.SelectMany(x => (x as MaterialAssetPropertyChangeEventArgs).ShaderPropertiesOriginalValues)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.First());
				customPayloads.Add(new MaterialAssetPropertyChangeEventArgs()
				{
					ID = r.Key,
					Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(r.Key)),
					Changes = changesDict,
					OriginalValues = originalValues,
					ShaderProperties = shaderChangesDict,
					ShaderPropertiesOriginalValues = ShaderOriginalValues
				});
			}

			this.FilteredPayloads = customPayloads.OfType<AssetPropertyChangeEventArgs>().ToList();
			return result.OfType<AssetPropertyChangeEventArgs>().ToList();
		}

		public override bool DrawValues(AssetPropertyChangeEventArgs change, List<float> columnWidths)
		{
			if (change is not MaterialAssetPropertyChangeEventArgs matChange ||
			    !matChange.ShaderProperties.Any()) return false;
			GUILayout.Label(change.Path, GUILayout.Width(columnWidths[0]));
			GUILayout.Label( "Shader Changes: " + string.Join(',', matChange.ShaderProperties.Select(x => x.Key)),
				GUILayout.Width(columnWidths[1]));
			return true;
		}

		public override bool PerformUndo(AssetPropertyChangeEventArgs payload)
		{
			if (payload is not MaterialAssetPropertyChangeEventArgs matPayload) return false;
			MaterialAssetUpdateChangeStrategy.Perform(matPayload.ShaderPropertiesOriginalValues, AssetDatabase.LoadAssetAtPath<Material>(matPayload.Path),matPayload.Path);
			return true;
		}
	}
}