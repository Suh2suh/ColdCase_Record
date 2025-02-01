using System.Collections;
using UnityEngine;


public class CursorManager
{
	CursorManager()
	{
		SceneInfo.OnSceneInfoUpdated += OnSceneInfoUpdated;
	}
	~CursorManager()
	{
		SceneInfo.OnSceneInfoUpdated -= OnSceneInfoUpdated;
	}


	private static void OnSceneInfoUpdated(SceneType newSceneType)
	{
		switch(newSceneType)
		{
			case SceneType.Sequence:
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				break;
			case SceneType.Lobby:
				Cursor.lockState = CursorLockMode.Confined;
				Cursor.visible = true;

				break;
			case SceneType.Game:

				break;
		}
	}


}