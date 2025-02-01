using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NpcAudioAssetGetter : StreamingAssetGetter
{
	MonoBehaviour senderMonoBehaviour;
	
	Dictionary<string, AudioClip> audioKeyContainer;
	public Dictionary<string, AudioClip> AudioKeyContainer { get => audioKeyContainer;  }

	//bool isAudioContainerLoaded;
	//public bool IsAudioContainerLoaded { get => isAudioContainerLoaded; }


	public NpcAudioAssetGetter(MonoBehaviour senderMonoBehaviour)
	{
		this.senderMonoBehaviour = senderMonoBehaviour;
		audioKeyContainer = new();
	}


	public IEnumerator LoadNpcPhaseAudio(string npcName, string phase)
	{
		string folderStructure = "/" + npcName + "/" + phase;
		var filePaths = GetAllFilePathsInFolder(folderStructure);

		//Debug.Log(filePaths);
		if(filePaths != null)
		{
			foreach (string filePath in filePaths)
			{
				//Debug.Log(filePath);
				yield return senderMonoBehaviour.StartCoroutine(LoadAudio(filePath));
			}
		}

		// Debug.Log("Audio Data Get Done");
	}



	IEnumerator LoadAudio(string filePath)
	{
		// TODO: 이후 파일 경로(파일 형식)에 따라 다르게 지정할 수 있도록 설정할 것
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG))
		{
			yield return www.SendWebRequest();

			if(www.result == UnityWebRequest.Result.Success)
			{
				AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

				audioKeyContainer[Path.GetFileNameWithoutExtension(filePath)] = audioClip;
				//Debug.Log(Path.GetFileNameWithoutExtension(filePath));
			}
			else
			{
				Debug.LogWarning("[REQUEST FAILED]: AudioFile Request Failed!");
			}

		}

	}



}