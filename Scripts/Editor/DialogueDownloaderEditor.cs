using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueDownloader))]
public class DialogueDownloaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var dialogueDownloader = target as DialogueDownloader;

		EditorGUILayout.LabelField("[DOWNLOAD] Common Dialogue", EditorStyles.boldLabel);
		if (GUILayout.Button("Narration Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("Narration");
			dialogueDownloader.DownloadNarrationData("WalkieTalkie");
		}
		if (GUILayout.Button("Setting Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("Setting");
		}

		if (GUILayout.Button("Item Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("ItemName");
			dialogueDownloader.DownloadNarrationData("ItemExplanation");
		}
		if (GUILayout.Button("Book Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("Book");
		}
		if (GUILayout.Button("InvestigationEvent Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("InvestigationEvent");
		}



		EditorGUILayout.Space();
		EditorGUILayout.LabelField("[DOWNLOAD] Npc Dialogue", EditorStyles.boldLabel);
		if (GUILayout.Button("NPC Dialogue"))
		{
			dialogueDownloader.DownloadNarrationData("Prank");
			dialogueDownloader.DownloadNarrationData("Miranda");
			dialogueDownloader.DownloadNarrationData("Laura");
		}



		EditorGUILayout.Separator();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("[DOWNLOAD] All Dialogue", EditorStyles.boldLabel);
		if (GUILayout.Button("ALL Dialogue"))
		{
			foreach(var sheetName in DataManager.dialogueUrlDictionary.Keys)
				dialogueDownloader.DownloadNarrationData(sheetName);
		}


	}

}
