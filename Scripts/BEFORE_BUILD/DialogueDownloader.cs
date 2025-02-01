using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public class DialogueDownloader : MonoBehaviour
{
	[Space(15)]
	[SerializeField] DialogueInfo dialogueInfo;


	public void DownloadNarrationData(string sheetName)
	{
		StartCoroutine(DownloadSheet(sheetName));
	}

	public IEnumerator DownloadSheet(string sheetName)
	{
		var sheetData = "";
		var sheetDownloadURL = DataManager.dialogueUrlDictionary[sheetName];

		using (UnityWebRequest www = UnityWebRequest.Get(sheetDownloadURL))
		{
			yield return www.SendWebRequest();

			if (www.isDone)
				sheetData = www.downloadHandler.text;
		}


		if(DialogueInfo.CommonDialogueSheetNames.Contains(sheetName))
			dialogueInfo.UpdateCommonDialogueDictionary(sheetName, CustomCSVReader.ReadCommonDialogue(sheetData));

		else
			dialogueInfo.UpdateNpcDialogueDictionary(sheetName, CustomCSVReader.ReadNpcDialogue(sheetData));


		//Debug.Log(sheetName + " Sheet Downloaded!");
	}


}




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
			foreach (var sheetName in DataManager.dialogueUrlDictionary.Keys)
				dialogueDownloader.DownloadNarrationData(sheetName);
		}


	}

}


