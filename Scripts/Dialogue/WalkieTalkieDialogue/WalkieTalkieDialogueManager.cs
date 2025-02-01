using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WalkieTalkieDialogueManager : MonoBehaviour
{

	// TODO: Place는 추후 무조건, 영역에 따라서 PlaceChecker이 받아오게 할 것
	[SerializeField]
	PlaceInfo placeInfo;

	[Header("Dialogue")]
	//[SerializeField] PlaceInfo placeInfo;
	[SerializeField] DialogueInfo dialogueInfo;
	UDictionary<string, UDictionary<Language, string>> walkieTalkieDialouge;
	bool isCommunicable;
	string latestWrittenKey; //시작 시 무조건 초기화되기때문에 ㄱㅊㄱㅊ

	[SerializeField] NarrationSubtitlePlayer narrationSubtitlePlayer;

	[Header("Audio")]
	[SerializeField] AudioSource narrationAudioSource;
	AudioSourceController audioSourceController;
	NpcAudioAssetGetter walkieTalkieAudioAssetGetter;

	[Header("Decision Event")]
	[SerializeField] DecisionEventController decisionEventController;


	string KeyCode = "End";


	#region Unity Methods

	private void Awake()
	{
		DialogueInfo.OnWalkieTalkieDialogueStart += StartWalkieTalkieDialogue;
		DialogueInfo.OnWalkieTalkieDialogueEnd += EndWalkieTalkieDialogue;
		decisionEventController.OnDecisionButtonClicked += OnDecisionButtonClicked;

		audioSourceController = new AudioSourceController(narrationAudioSource);

		walkieTalkieDialouge = dialogueInfo.CommonDialogueDictionary["WalkieTalkie"];
		isCommunicable = false;
	}

	private void Start()
	{
		LoadWalkieTalkieAudioAsset();
	}

	private void Update()
	{
		if (PlayerStatusManager.CurrentInterStatus == InteractionStatus.TalkingWalkieTalkie)
		{
			if (isCommunicable && Input.GetMouseButtonDown(0))
				ManageWalkieTalkieDialogue();
		}
	}

	private void OnDestroy()
	{
		DialogueInfo.OnWalkieTalkieDialogueStart -= StartWalkieTalkieDialogue;
		DialogueInfo.OnWalkieTalkieDialogueEnd += EndWalkieTalkieDialogue;
		decisionEventController.OnDecisionButtonClicked -= OnDecisionButtonClicked;
	}

	#endregion


	void StartWalkieTalkieDialogue()
	{
		narrationSubtitlePlayer.ActivateNarrationPanel(true);

		string firstKey;
		if (PhaseChecker.GetCurrentPhase() < 'D' || PhaseChecker.GetCurrentPhase() == 'Z')
			firstKey = KeyCode + "_Error";
		else
			firstKey = KeyCode + "_1";

		if (placeInfo.isPlaceCleared == true)
		{
			narrationSubtitlePlayer.ShowSubtitleConstantly("WalkieTalkie", "Glitch");
			latestWrittenKey = "_Error";
		}
		else
			CreateWalkieTalkieDialogue(firstKey);
		

		isCommunicable = true;
	}


	void ManageWalkieTalkieDialogue()
	{
		string prevKey = latestWrittenKey;
		if (placeInfo.isPlaceCleared == true || prevKey.Contains("_error") || prevKey.Contains("_Error"))
		{
			DialogueInfo.OnWalkieTalkieDialogueEnd();
			return;
		}
		if(GetKeyIndexInData(prevKey, walkieTalkieDialouge) >= walkieTalkieDialouge.Count - 1)
		{
			placeInfo.isPlaceCleared = true;

			DialogueInfo.OnWalkieTalkieDialogueEnd();
			return;
		}


		string newKey = (prevKey.Contains("_answer") ? GetNextKeyOfResultKey(prevKey) : GetNextKeyByIndex(prevKey, walkieTalkieDialouge));
		if(newKey.Split('_').Length == 3)
		{
			decisionEventController.InvokeDecisionEventWith("WalkieTalkie", GetDecisionKeys(newKey));
			isCommunicable = false;
			return;
		}

		CreateWalkieTalkieDialogue(newKey);
	}

	void EndWalkieTalkieDialogue()
	{
		isCommunicable = false;

		Debug.Log(narrationSubtitlePlayer);

		narrationSubtitlePlayer.ActivateNarrationPanel(false);
		decisionEventController.DestroyDecisionPanel();

		if(narrationAudioSource) narrationAudioSource.Stop();
	}




	#region Decision Event


	List<string> GetDecisionKeys(string decisionStartKey)
	{
		List<string> decisionKeys = new();

		string decisionKeyParse = decisionStartKey;
		do
		{
			decisionKeys.Add(decisionKeyParse);
			decisionKeyParse = GetNextKeyByIndex(decisionKeyParse, walkieTalkieDialouge);
		} while (decisionKeyParse.Split('_').Length == 3);

		return decisionKeys;
	}


	void OnDecisionButtonClicked(string clickedButtonName)
	{
		decisionEventController.DestroyDecisionPanel();

		CreateWalkieTalkieDialogue(GetResultKeyOfDecision(clickedButtonName, walkieTalkieDialouge));
		isCommunicable = true;
	}

	#endregion

	#region Audio

	void LoadWalkieTalkieAudioAsset()
	{
		walkieTalkieAudioAssetGetter = new NpcAudioAssetGetter(this);

		StartCoroutine(walkieTalkieAudioAssetGetter.LoadNpcPhaseAudio("WalkieTalkie", KeyCode));
	}

	void PlayWalkieTalkieAudio(string writtenKey)
	{

		//Debug.Log(writtenKey + walkieTalkieAudioAssetGetter.AudioKeyContainer.ContainsKey(writtenKey));

		string audioKey;
		var splits = writtenKey.Split('_');
		if (splits.Length == 4 && ! walkieTalkieAudioAssetGetter.AudioKeyContainer.ContainsKey(writtenKey))
		{
			// 본래 키가 없다면, 14_a_answer => 14_answer 불러오기 (중복 오디오라서)
			audioKey = splits[0] + "_" + splits[1] + "_" + splits[3];
		}
		else
			audioKey = writtenKey;


		if (walkieTalkieAudioAssetGetter.AudioKeyContainer.ContainsKey(audioKey))
			audioSourceController.PlayAudioClip(walkieTalkieAudioAssetGetter.AudioKeyContainer[audioKey]);

		//End_13_a_answer 가 아니라 End_
	}



	#endregion



	void CreateWalkieTalkieDialogue(string key)
	{
		PlayWalkieTalkieAudio(key);
		narrationSubtitlePlayer.ShowSubtitleConstantly("WalkieTalkie", key);

		latestWrittenKey = key;
	}



	public static string GetResultKeyOfDecision(string selectKey, UDictionary<string, UDictionary<Language, string>> targetNpcData)
	{
		if (targetNpcData.Keys.Contains(selectKey + "_error"))
			return (selectKey + "_error");

		if (targetNpcData.Keys.Contains(selectKey + "_answer"))
			return (selectKey + "_answer");

		return null;
	}

	 string GetNextKeyOfResultKey(string answerKey)
	{
		var splits = answerKey.Split('_');


		return splits[0] +"_" + (int.Parse(splits[1]) + 1).ToString();
	}


	// TODO: 옛날 NPC DIALOGUE KEY GETTER 썼음 : 나중에 따로 빼기. 지금은 시간없
	// Key Getter 그냥 Dialogue에 넣은 다이얼로그와 키에 따라 알아서 갖고오게 만들기
	public static string GetNextKeyByIndex(string key, UDictionary<string, UDictionary<Language,string>> targetNpcData)
	{
		int currentKeyIndex = GetKeyIndexInData(key, targetNpcData);
		int nextKeyIndex = currentKeyIndex + 1;

		return targetNpcData.Keys[nextKeyIndex];
	}


	public static int GetKeyIndexInData(string key, UDictionary<string, UDictionary<Language, string>> targetNpcData)
	{
		return targetNpcData.Keys.IndexOf(key);
	}


}
