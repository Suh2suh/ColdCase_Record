using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// This should be attatch to all scenes for check
/// </summary>
public class SceneChecker : Singleton<SceneChecker>
{
	[SerializeField] private SceneInfo sceneInfo;

	protected override void Awake()
	{
		base.Awake();

		// since OnSceneLoaded is slower than Awake(), for the first time try here
		var sceneName = SceneManager.GetActiveScene().name;
		sceneInfo.UpdateCurrentSceneInfo(newSceneName: sceneName);

		if (sceneName.Contains("Sequence"))
		{
			sceneInfo.UpdateCurrentSceneInfo(SceneType.Sequence);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else if (sceneName.Contains("Lobby"))
		{
			sceneInfo.UpdateCurrentSceneInfo(SceneType.Lobby);
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}
		else if (sceneName.Contains("Stage"))
		{
			sceneInfo.UpdateCurrentSceneInfo(SceneType.Game);
		}


		SceneManager.sceneLoaded += OnSceneLoaded;
	}


	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		sceneInfo.UpdateCurrentSceneInfo(newSceneName: scene.name);
	}


	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}


}
