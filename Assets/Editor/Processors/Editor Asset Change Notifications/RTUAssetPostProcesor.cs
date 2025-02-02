using RTUEditor.ObjectChange;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class RTUAssetPostProcesor : AssetPostprocessor
	{
		private static EditorRtuController editorRtuController;
		private static AssetPropertyChangeRTUEditorProcessor processor;
		public static bool Surpress { get; set; } = false;

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths, bool reload)
		{
			if (reload || !editorRtuController.IsConnected || Surpress) return;
			foreach (string path in importedAssets)
			{
				Debug.Log("Reimported Asset: " + path);
				var asset = AssetDatabase.LoadMainAssetAtPath(path);
				processor.Process(asset);
			}
		}

		public static void Init(EditorRtuController editorRtuController)
		{
			RTUAssetPostProcesor.editorRtuController = editorRtuController;
			processor = new AssetPropertyChangeRTUEditorProcessor(editorRtuController);
		}
	}
}