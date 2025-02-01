using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
	// TODO: 추후, place가 많아지면 current Place 지정, 해당 place데이터만 불러오면 됨.
	// 이전 place Data(필요하다면 로드 ,필요없다면 그냥 isCleared만 불러와도 됨)

	// <Place>
	// - phase
	// - isCleared?
	// - evidences: takenPhoto
	// - evidences: obtainedItems (obtain: inventoryBox에서 로드하면 됨, isChecked: 저장 필요)
	// - npcs: talkedNpcKey, askedNpcQuestion


	// TODO: 추후, PlayerInfo 데이터 먼저 싹 가져온 이후, 탐색해야함
	[SerializeField] TutorialInfo tutorialInfo;
	[SerializeField] PlayerInfo playerInfo;
	[SerializeField] DialogueInfo dialogueInfo;

	/// <summary>  On Place Clear시, 다음 Place로 currentPlace 전환.  </summary>
	PlaceInfo currentPlace;
	/// <summary>  게임 시작 시 (awake)에서 초기화됨  </summary>
	public static PlaceInfo CurrentPlace;
	public bool isNewGame;



	#region Unity Methods

	protected override void Awake()
	{
		PhaseController.OnPhaseUpdated += SaveGame;
		PlaceInfo.OnPlaceCleared += SaveGame;

		base.Awake();
		DontDestroyOnLoad(this.gameObject);

		isNewGame = true;

		currentPlace = playerInfo.CurrentPlace;
	}

	private void OnDestroy()
	{
		PhaseController.OnPhaseUpdated -= SaveGame;
		PlaceInfo.OnPlaceCleared -= SaveGame;
	}


	#endregion



	void SaveGame(PlaceInfo placeInfo)
	{
		StartCoroutine(SaveAll());
	}

	public IEnumerator SaveAll()
	{
		//yield return StartCoroutine(SavePlayerDataAsync());
		yield return StartCoroutine(SaveTutorialrDataAsync());
		yield return StartCoroutine(SaveDialogueHighlightDataAsync());
		yield return StartCoroutine(SavePlaceDataAsync());
	}
	//async Task SaveAll()
	//{
	//	await SavePlayerDataAsync();
	//	await SaveDialogueHighlightDataAsync();
	//	await SavePlaceDataAsync();
	//}
	// 새게임시작시
	public void InitializeAll()
	{
		//playerInfo.CurrentPlace = playerInfo.GamePlaceInSequence[0];
		currentPlace = playerInfo.CurrentPlace;
		InitializePlaceData();

		tutorialInfo.IsTutorialEnd = false;
		dialogueInfo.CleanNpcDialogue();
	}

	public void LoadAll()
	{
		if (File.Exists(GetPlayerDataPath()))
			LoadTutorialData();
		currentPlace = playerInfo.CurrentPlace;


		if (File.Exists(GetPlaceSaveDataPath()))
			LoadPlaceData();
		else
			InitializePlaceData();

		if (File.Exists(GetNpcDialogueDataPath()))
			LoadDialogueHighlightData();
		else
			dialogueInfo.CleanNpcDialogue();
	}


	IEnumerator SaveTutorialrDataAsync()
	{
		//string playerJsonDataContent = JsonUtility.ToJson(tutorialInfo);
		string playerJsonDataContent = JsonConvert.SerializeObject(tutorialInfo, Formatting.Indented);

		string directory = Path.GetDirectoryName(GetPlayerDataPath());
		if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);


		StreamWriter writer = null;
		try
		{
			writer = new StreamWriter(GetPlayerDataPath());
			yield return writer.WriteAsync(playerJsonDataContent);
		}
		finally
		{
			if (writer != null)
				writer.Dispose();
		}


		Debug.Log("[Saved]: " + playerJsonDataContent);
	}

	void LoadTutorialData()
	{
		string playerJsonDataContent = DataManager.ReadData(GetPlayerDataPath());

		//PlayerInfo tempDataContainer = ScriptableObject.CreateInstance<PlayerInfo>();
		TutorialInfo tempDataContainer = JsonConvert.DeserializeObject<TutorialInfo>(playerJsonDataContent);
		//JsonUtility.FromJsonOverwrite(playerJsonDataContent, tempDataContainer);

		Debug.Log(playerJsonDataContent);
		Debug.Log("Tutorial End: " + tempDataContainer.IsTutorialEnd);

		tutorialInfo.SetTutorialStatus(tempDataContainer.IsTutorialEnd);



	}

	/*

	IEnumerator SavePlayerDataAsync()
	{
		string playerJsonDataContent = JsonUtility.ToJson(playerInfo);
		//DataManager.WriteData(GetPlayerDataPath(), playerJsonDataContent);

		Debug.Log(playerJsonDataContent);

		string directory = Path.GetDirectoryName(GetPlayerDataPath());
		if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

		using (StreamWriter writer = new StreamWriter(GetPlayerDataPath()))
		{
			yield return writer.WriteAsync(playerJsonDataContent);
		}


		//Debug.Log("Tutorial: " + playerInfo.tutorialInfo.IsTutorialEnd);
		//Debug.Log("[Saved]: " + currentPlace.name + " / " + playerJsonDataContent);
		Debug.Log("[Saved]: " + playerJsonDataContent);
	}

	void LoadPlayerData()
	{
		string playerJsonDataContent = DataManager.ReadData(GetPlayerDataPath());

		//PlayerInfo tempDataContainer = ScriptableObject.CreateInstance<PlayerInfo>();
		PlayerInfo tempDataContainer = JsonConvert.DeserializeObject<PlayerInfo>(playerJsonDataContent);
		JsonUtility.FromJsonOverwrite(playerJsonDataContent, tempDataContainer);

		if (tempDataContainer.CurrentPlace == null)
			playerInfo.CurrentPlace = playerInfo.GamePlaceInSequence[0];
		else
			playerInfo.CurrentPlace = tempDataContainer.CurrentPlace;



	}

	*/
	#region PlaceInfo

	IEnumerator SavePlaceDataAsync()
	{
		//string placeJsonDataContent = JsonUtility.ToJson(currentPlace);
		string placeJsonDataContent = JsonConvert.SerializeObject(currentPlace, Formatting.Indented);

		string directory = Path.GetDirectoryName(GetPlaceSaveDataPath());
		if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);


		StreamWriter writer = null;
		try
		{
			writer = new StreamWriter(GetPlaceSaveDataPath());
			yield return writer.WriteAsync(placeJsonDataContent);
		}
		finally
		{
			if (writer != null)
				writer.Dispose();
		}

		/*
		Debug.Log("[Saved]: " + currentPlace.name + " / " + placeJsonDataContent);
		foreach (var a in currentPlace.NotebookInPlace.AskedQuestionSubjectPerNpc)
		{
			Debug.Log(a.Key);
			foreach (var b in a.Value.questionList)
				Debug.Log(b);
		}
		foreach (var a in currentPlace.NotebookInPlace.ChapterRecordPerNpc)
			foreach (var b in a.Value.pages)
				foreach (var c in b.keysPerPage)
					Debug.Log(c);
		foreach (var a in currentPlace.NotebookInPlace.LatestDialogueKeyPerNpc)
			Debug.Log(a.Value + " / " + a.Key);*/
	}


	void LoadPlaceData()
	{
		string placeJsonDataContent = DataManager.ReadData(GetPlaceSaveDataPath());

		//PlaceInfo tempDataContainer = ScriptableObject.CreateInstance<PlaceInfo>();
		PlaceInfo tempDataContainer = JsonConvert.DeserializeObject<PlaceInfo>(placeJsonDataContent);
		//JsonUtility.FromJsonOverwrite(placeJsonDataContent, tempDataContainer);


		var originalPlaceData = currentPlace;
		originalPlaceData.Phase = tempDataContainer.Phase;
		originalPlaceData.isPlaceCleared = tempDataContainer.isPlaceCleared;


		for(int i = 0; i < originalPlaceData.AllEvidencesInPlace.Count; i++)
		{
			//Debug.Log(tempDataContainer.AllEvidencesInPlace);

			var originalEvidenceData = originalPlaceData.AllEvidencesInPlace[i];
			var loadedEvidenceData = tempDataContainer.AllEvidencesInPlace[i];

			//Debug.Log(originalEvidenceData.name);
			//Debug.Log(loadedEvidenceData.name);

			originalEvidenceData.SetIsObtained(loadedEvidenceData.IsObtained);
			originalEvidenceData.SetIsChecked(loadedEvidenceData.IsChecked);
		}

		//if (tempDataContainer.NotebookInPlace.AskedQuestionSubjectPerNpc != null)
			originalPlaceData.NotebookInPlace.AskedQuestionSubjectPerNpc = tempDataContainer.NotebookInPlace.AskedQuestionSubjectPerNpc;
		//else
		//	originalPlaceData.NotebookInPlace.AskedQuestionSubjectPerNpc = new();

		//if (tempDataContainer.NotebookInPlace.ChapterRecordPerNpc != null)
			originalPlaceData.NotebookInPlace.ChapterRecordPerNpc = tempDataContainer.NotebookInPlace.ChapterRecordPerNpc;
		//else
		//	originalPlaceData.NotebookInPlace.ChapterRecordPerNpc = new();


		originalPlaceData.NotebookInPlace.LatestDialogueKeyPerNpc = tempDataContainer.NotebookInPlace.LatestDialogueKeyPerNpc;


		/*
		foreach (var a in tempDataContainer.NotebookInPlace.AskedQuestionSubjectPerNpc)
		{
			Debug.Log(a.Key);
			foreach (var b in a.Value.questionList)
				Debug.Log(b);
		}
		foreach (var a in tempDataContainer.NotebookInPlace.ChapterRecordPerNpc)
			foreach (var b in a.Value.pages)
				foreach (var c in b.keysPerPage)
					Debug.Log(c);
		foreach (var a in tempDataContainer.NotebookInPlace.LatestDialogueKeyPerNpc)
			Debug.Log(a.Value + " / " + a.Key);


		foreach (var a in originalPlaceData.NotebookInPlace.AskedQuestionSubjectPerNpc)
		{
			Debug.Log(a.Key);
			foreach (var b in a.Value.questionList)
				Debug.Log(b);
		}
		foreach (var a in originalPlaceData.NotebookInPlace.ChapterRecordPerNpc)
			foreach (var b in a.Value.pages)
				foreach (var c in b.keysPerPage)
					Debug.Log(c);
		foreach (var a in originalPlaceData.NotebookInPlace.LatestDialogueKeyPerNpc)
			Debug.Log(a.Value + " / " + a.Key);
		*/

	}

	void InitializePlaceData()
	{
		var originalPlaceData = currentPlace;
		originalPlaceData.Phase = 'Z';
		originalPlaceData.isPlaceCleared = false;
		for (int i = 0; i < originalPlaceData.AllEvidencesInPlace.Count; i++)
		{
			var originalEvidenceData = originalPlaceData.AllEvidencesInPlace[i];

			originalEvidenceData.SetIsObtained(false);
			originalEvidenceData.SetIsChecked(false);
		}
		originalPlaceData.NotebookInPlace.ResetDialogue();

		Debug.Log("[Reset]: " + currentPlace.name);
	}


	#endregion


	#region Dialogue(Npc)


	// todo: place별로 npc dialogue 나눠서, place에 마찬가지로 형광펜 기록 저장하기

	IEnumerator SaveDialogueHighlightDataAsync()
	{
		string npcDialogueJsonDataContent = JsonConvert.SerializeObject(dialogueInfo.NpcDialogueDictionary);

		string directory = Path.GetDirectoryName(GetNpcDialogueDataPath());
		if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);


		StreamWriter writer = null;
		try
		{
			writer = new StreamWriter(GetNpcDialogueDataPath());
			yield return writer.WriteAsync(npcDialogueJsonDataContent);
		}
		finally
		{
			if (writer != null)
				writer.Dispose();
		}


		Debug.Log("[Saved]: " + npcDialogueJsonDataContent);
	}

	void LoadDialogueHighlightData()
	{
		string npcDialogueJsonDataContent = File.ReadAllText(GetNpcDialogueDataPath());

		UDictionary<string, UDictionary<string, CommunicationData>> loadedNpcDialogue;
		try
		{
			loadedNpcDialogue = Newtonsoft.Json.JsonConvert.DeserializeObject<UDictionary<string, UDictionary<string, CommunicationData>>>(npcDialogueJsonDataContent);
			dialogueInfo.NpcDialogueDictionary = loadedNpcDialogue;
		}
		catch (System.Exception)
		{
			dialogueInfo.CleanNpcDialogue();

			throw;
		}
	}


	#endregion



	string GetPlaceSaveDataPath()
	{
		//string placeSaveFileName = currentPlace.name + ".json";
		string placeSaveFileName = "Tutorial" + ".json";
		//return (Path.Combine(DataManager.GetPlaceDataFolderPath(currentPlace), placeSaveFileName));
		return (Path.Combine(DataManager.GetPlaceDataFolderPath(), placeSaveFileName));
	}

	string GetNpcDialogueDataPath()
	{
		string npcDialogueFileName = "npcDialogue.json";
		return (Path.Combine(DataManager.saveDataPath, npcDialogueFileName));
	}
	string GetPlayerDataPath()
	{
		string npcDialogueFileName = "playerData.json";
		return (Path.Combine(DataManager.saveDataPath, npcDialogueFileName));
	}


}
