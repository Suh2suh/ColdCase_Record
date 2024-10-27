using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// This should be attatch to all scenes fo checking
/// </summary>
public class SceneChecker : Singleton<SceneChecker>
{
	[SerializeField] SceneInfo sceneInfo;

	protected override void Awake()
	{
		base.Awake();

		var sceneName = SceneManager.GetActiveScene().name;

		if (sceneName.Contains("Sequence"))
		{
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Sequence);
			Cursor.lockState = CursorLockMode.Locked;   // cursorÀº µû·Î »©±â
			Cursor.visible = false;
		}
		else if (sceneName.Contains("Lobby"))
		{
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Lobby);
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}
		else if (sceneName.Contains("Stage"))
		{
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Game);

		}

		//DontDestroyOnLoad(this);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}


	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{

		if (scene.name.Contains("Sequence"))
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Sequence);
		else if (scene.name.Contains("Lobby"))
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Lobby);
		else if (scene.name.Contains("Stage"))
			sceneInfo.SetCurrentSceneInfo(SceneInfo.Scene.Game);
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}


}
