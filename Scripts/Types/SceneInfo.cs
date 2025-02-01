using System;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneInfo", menuName = "ScriptableObjects/Informations/SceneInfo", order = 1)]
public class SceneInfo : ScriptableObject
{
	#region Private Variables
	[SerializeField] private SceneType currentSceneType;
	private SceneType previousSceneType = SceneType.None;

	#endregion
	public SceneType CurrentSceneType => currentSceneType;
	public SceneType PreviousSceneType => previousSceneType;

	public static Action<SceneType> OnSceneInfoUpdated;


	public void UpdateCurrentSceneInfo(SceneType newSceneType)
	{
		if (newSceneType == currentSceneType)
			return;

		previousSceneType = currentSceneType;
		currentSceneType = newSceneType;
		OnSceneInfoUpdated?.Invoke(currentSceneType);
	}
	public void UpdateCurrentSceneInfo(string newSceneName)
	{
		if (newSceneName.Contains("Sequence"))
			UpdateCurrentSceneInfo(SceneType.Sequence);
		else if (newSceneName.Contains("Lobby"))
			UpdateCurrentSceneInfo(SceneType.Lobby);
		else if (newSceneName.Contains("Stage"))
			UpdateCurrentSceneInfo(SceneType.Game);
	}


}
