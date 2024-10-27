using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneInfo", menuName = "ScriptableObjects/Informations/SceneInfo", order = 1)]
public class SceneInfo : ScriptableObject
{
	public enum Scene
	{
		Lobby, Game, Sequence
	};

	[SerializeField] Scene currentScene;

	public void SetCurrentSceneInfo(Scene scene)
	{
		currentScene = scene;
	}
	public Scene GetCurrentSceneInfo()
	{
		return currentScene;
	}
}
