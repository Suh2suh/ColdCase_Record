using System.Collections;
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
