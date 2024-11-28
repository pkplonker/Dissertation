using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
	public class RTUScene
	{
		private RTUSceneStage customStage;

		public void ShowScene()
		{
			customStage = ScriptableObject.CreateInstance<RTUSceneStage>();
			StageUtility.GoToStage(customStage, true);
			var scene = customStage.scene;
			Scene currentScene = SceneManager.GetActiveScene();
			GameObject[] rootObjects = currentScene.GetRootGameObjects();
			foreach (GameObject rootObj in rootObjects)
			{
				GameObject clonedObject = GameObject.Instantiate(rootObj);
				clonedObject.name = rootObj.name;
				SceneManager.MoveGameObjectToScene(clonedObject, scene);
				clonedObject.transform.position = rootObj.transform.position;
			}
		}

		public bool IsVisible() => StageUtility.GetCurrentStage() == customStage;

		public void Close()
		{
			if (IsVisible())
			{
				// bool result = EditorUtility.DisplayDialog(
				// 	"Copy RTU changes back to scene?",
				// 	"Copy RTU changes back to scene?",
				// 	"Yes",
				// 	"No"
				// );
				//
				// if (result)
				// {
				// 	
				// }
				// else
				// {
				// 	
				// }
				StageUtility.GoToMainStage();
			}
		}
	}
}