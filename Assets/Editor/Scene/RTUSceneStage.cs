using UnityEditor.SceneManagement;
using UnityEngine;

namespace RTUEditor
{
	public class RTUSceneStage : PreviewSceneStage
	{
		protected override GUIContent CreateHeaderContent() => new GUIContent("RTU Preview Scene");

		protected override void OnDisable()
		{
			base.OnDisable();

			if (scene.IsValid())
			{
				EditorSceneManager.CloseScene(scene, true);
			}
		}
	}
}