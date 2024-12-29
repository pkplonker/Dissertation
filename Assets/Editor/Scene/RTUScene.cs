using System.Threading.Tasks;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTUEditor
{
	public class RTUScene
	{
		private RTUSceneStage customStage;
		private readonly TaskScheduler scheduler;

		public RTUScene(TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		public void ShowScene()
		{
			ThreadingHelpers.ActionOnScheduler(() =>
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
			}, scheduler);
		}

		public bool IsVisible() => StageUtility.GetCurrentStage() == customStage;
		public Scene? GetScene() => customStage?.scene;

		public void Close()
		{
			ThreadingHelpers.ActionOnScheduler(() =>
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
			}, scheduler,1000);
		}
	}
}