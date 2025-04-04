﻿using System;
using System.Threading.Tasks;
using RealTimeUpdateRuntime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTUEditor
{
	public class RTUScene
	{
		private RTUSceneStage customStage;
		private readonly TaskScheduler scheduler;
		public event Action<Scene?> SceneCreated; 
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

				SceneCreated?.Invoke(scene);
			}, scheduler);
		}

		public bool IsVisible() => StageUtility.GetCurrentStage() == customStage;
		public Scene? GetScene() => customStage?.scene;

		public async void Close()
		{
			await ThreadingHelpers.ActionOnSchedulerAsync(() =>
			{
				if (IsVisible())
				{
					StageUtility.GoToMainStage();
				}
			}, scheduler);
		}
	}
}