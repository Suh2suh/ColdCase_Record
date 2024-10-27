using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameResetter))]
public class GameResetterEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var gameResetter = target as GameResetter;

		if (GUILayout.Button("[RESET] Inventory"))
		{
			gameResetter.ResetInventory();
		}
		if (GUILayout.Button("[RESET] Photo Evidence"))
		{
			gameResetter.ResetPhotoEvidences();
		}
		if(GUILayout.Button("[RESET] Tutorial"))
		{
			gameResetter.ResetTutorial();
		}
		if(GUILayout.Button("[RESET] Dialogue"))
		{
			gameResetter.ResetNpcDialogueWithPECleared();
		}
		if (GUILayout.Button("[CLEAN] Dialogue Highlight"))
		{
			gameResetter.CleanNpcDialogueHighlight();
		}
	}


}
