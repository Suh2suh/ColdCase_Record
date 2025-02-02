using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using ColdCase.Dialogue.Book.Utility;
using System.IO;
using System.Collections;

public class NpcDialogueManager : MonoBehaviour
{
	#region Variables

	#region SerializeFields

	[SerializeField] DialogueInfo dialogueInfo;

	[SerializeField, Space(15)] GameObject panelFolderPrefab;
	[SerializeField] GameObject npcDialoguePanelPrefab;
	[SerializeField] GameObject npcTextPrefab;
	[SerializeField] GameObject playerTextPrefab;
	[SerializeField] GameObject spacerTextPrefab;

	[SerializeField, Space(15)] AudioSource playerAuidoSource;

	#endregion

	#region Book Utility

	BookHighlighter bookHighlighter;

	BookChapterIndexerCreator bookChapterIndexerCreator;
	BookChapterNavigator bookChapterNavigator;
	BookPageNavigator bookPageNavigator;

	bool isBookChapterNavigatable = false;


	#endregion


	PlaceInfo placeInfo;

	string communicatingNpcName;
	UDictionary<string, CommunicationData> communicatingNpcData;

	/// <summary>  Notebook Controller가 대화하는 npc의 placea를 받아, notebookInfo를 다르게 전송해줌 </summary>
	NotebookInfo notebookInfo;
	Transform notebookCanvas;

	/// <summary> Npc별 대화 페이지 Panel들을 담는 저장소 </summary>
	Dictionary<string, ChapterObject> chapterObjectPerNpc;
	/// <summary> 직전에 노트북에 적힌 dialogue Key </summary>
	string latestWirttenDialogueKey = "";

	/// <summary>  T: 대화 생성 가능 / F: 대화 생성 불가  </summary>
	bool isCommunicable;

	// bool isDecisionEventOn;


	class ChapterObject
	{
		/// <summary> Parent Object of all page objects in npc's Chapter </summary>
		public GameObject chapterObj;
		public List<GameObject> pageObjects;

		public ChapterObject()
		{
			pageObjects = new();
		}
	}




	#endregion

	#region Unity Methods

	private void Awake()
	{
		NotebookInfo.OnNotebookCreated += PreprocessNotebookObjects;
		DialogueInfo.OnNpcClicked += LoadCommunicatingNpcData;
		DialogueInfo.OnNpcDialogueStart += StartNpcDialogue;
		DialogueInfo.OnNpcDialogueEnd += PostprocessNpcDialogue;

		detailVideoPlayController.OnVideoPlayFinished += OnDetailVideoPlayFinished;
	}

	private void OnDestroy()
	{
		NotebookInfo.OnNotebookCreated -= PreprocessNotebookObjects;
		DialogueInfo.OnNpcClicked -= LoadCommunicatingNpcData;
		DialogueInfo.OnNpcDialogueStart -= StartNpcDialogue;
		DialogueInfo.OnNpcDialogueEnd -= PostprocessNpcDialogue;

		detailVideoPlayController.OnVideoPlayFinished -= OnDetailVideoPlayFinished;
	}

	

	private void FixedUpdate()
	{
		if (PlayerStatusManager.CurrentInterStatus == InteractionStatus.TalkingNpc &&
			GameModeManager.CurrentGameMode == GameMode.InGame)
		{
			if(isBookChapterNavigatable) bookChapterNavigator.CheckPointingIndexerOnFixedUpdate();
		}
	}

	private void Update()
	{
		if (PlayerStatusManager.CurrentInterStatus == InteractionStatus.TalkingNpc &&
			GameModeManager.CurrentGameMode == GameMode.InGame)
		{

			if (isCommunicable && Input.GetMouseButtonDown(0))
				ManageNpcDialogue();


			if (isNpcTalking)
			{
				isNpcTalking = npcAudioSourceController.IsAudioSourcePlaying();
				if ( ! isNpcTalking) CloseCommunicatingNpcMouse();
			}


			if ( ! IsInvesitgationDialogueLeft() )
			{
				if (isBookChapterNavigatable)
					bookChapterNavigator.ManageChapterNavigationOnUpdate();

				if (bookHighlighter != null)
					bookHighlighter.ManageTextHighlightOnUpdate();

				if ( ! bookPageNavigator.isBookPageNavigatable)
					bookPageNavigator.isBookPageNavigatable = true;
			}
			else
			{
				if (bookPageNavigator.isBookPageNavigatable)
					bookPageNavigator.isBookPageNavigatable = false;
			}

		}
	}



	#endregion



 #region Main Dialogue Function


	/// <summary>  Invoked on click  </summary>
	void ManageNpcDialogue()
	{
		//Debug.Log("Click : " + isCommunicable + " / " + IsNavigatingFinalPageOfCommunicatingNpc() );
		if (!isCommunicable || !IsNavigatingFinalPageOfCommunicatingNpc()) return;		


		string prevDialogueKey = latestWirttenDialogueKey;
		string newDialogueKey = NpcDialogueKeyGetter.GetNextKeyInNpcSheet(prevDialogueKey, communicatingNpcData);

		bool isPrevKeyUpdatable = !NpcDialogueKeyChecker.IsErrorKey(prevDialogueKey);
		if (isPrevKeyUpdatable) notebookInfo.UpdateLatestKeyOf(communicatingNpcName, prevDialogueKey);


		//Debug.Log("next Key: " + newDialogueKey);
		// 다음 취조문이 끊겼으면, 새로 취조 질문 해야 하기 때문 -> 대화 중지 
		if(newDialogueKey == null)
		{
			DisableCommunication();

			return;
		}

		CreateDialogueWith(newDialogueKey);
	}



	void StartNpcDialogue()
	{
		investigationEventController.ActivateDecisionPanel(true);
		isCommunicable = true;
	}


	// key가 investigationkey인지, 일반 대화 key인지 구분해야함(_text로 끝나는지)
	void CreateDialogueWith(string key)
	{
		CreateDialogueUI(key);
		CreateDialogueTalk(key);
	}


	void OnNewChapterCreated()
	{
		InitializePageNavigatingIndex(communicatingNpcName);
	}

	void OnNewPageCreated()
	{
		UpdatePageNavigatingIndex();
	}



	#region Investigation Event

	[SerializeField] DecisionEventController investigationEventController;
	List<string> questionKeys;
	Color deActivatedColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
	readonly string investigationButtonTextParse = "_text";

	[SerializeField] VideoPlayController_WithSubtitle detailVideoPlayController;



	void CreateInvestigationEventForFirstTime(bool isTransparent)
	{
		if (IsInvesitgationDialogueLeft()) EnableCommunication();
		else										        DisableCommunication();

		bool isNotFirstTime = ( questionKeys != null && questionKeys.Count > 0 );
		if (isNotFirstTime)   return;


		questionKeys = CreateQuestionKeys();

		CreateInvestigationButtons();
		DeActivateAskedInvestigationButton(questionKeys);


		void CreateInvestigationButtons()
		{
			investigationEventController.CreateDecisionButtonsWith(questionKeys, isTransparent);
			investigationEventController.OnDecisionButtonClicked += OnInvestigationButtonClicked;
			foreach (var questionButton in investigationEventController.decisionButtons)
				LinkCommunicationLimiterTo(questionButton.transform);
		}

	}


	List<string> CreateQuestionKeys()
	{
		var newQuestionKeys = new List<string>();

		foreach (var basicQuestionKey in DialogueInfo.basicInvestigationQuestions)
			newQuestionKeys.Add('A' + "_" + basicQuestionKey);

		if (currentPhase >= 'C')
		{
			foreach (var evidence in placeInfo.AllEvidencesInPlace)
				newQuestionKeys.Add(currentPhase + "_" + evidence.name);
		}


		return newQuestionKeys;
	}


	void DeActivateAskedInvestigationButton(List<string> questionKeys)
	{
		if (notebookInfo.AskedQuestionSubjectPerNpc == null || ! notebookInfo.AskedQuestionSubjectPerNpc.ContainsKey(communicatingNpcName))
			return;

		foreach(var questionKey in questionKeys)
		{
			string questionSubject = GetInvestigationSubject(questionKey);

			if (notebookInfo.AskedQuestionSubjectPerNpc[communicatingNpcName].questionList.Contains(questionSubject))
				DeActiveQuestionButton(questionKey);
		}
	}


	void OnInvestigationButtonClicked(string clickedDecisionButtonName)
	{
		if (IsInvesitgationDialogueLeft())   return;


		string investigationSubject = GetInvestigationSubject(clickedDecisionButtonName);
		bool isAskedInvestigationSubject = notebookInfo.AskedQuestionSubjectPerNpc.ContainsKey(communicatingNpcName) &&
														   notebookInfo.AskedQuestionSubjectPerNpc[communicatingNpcName].questionList.Contains(investigationSubject);
		if (isAskedInvestigationSubject)
			OnAskedInvestigationButtonClicked(clickedDecisionButtonName);
		else
			OnUnAskedInvestigationButtonClicked(clickedDecisionButtonName);

	}


	void OnUnAskedInvestigationButtonClicked(string clickedInvestigationKey)
	{
		if ( IsInvesitgationDialogueLeft() || IsQuestionAllAsked(communicatingNpcName, currentPhase) ) return;


		// Flip Chapter / Page To 
		if (bookChapterNavigator.NavigatingChapterName != communicatingNpcName)
			bookChapterNavigator.NavigateNpcChapterTo(communicatingNpcName);
		if(chapterObjectPerNpc[communicatingNpcName].pageObjects != null && chapterObjectPerNpc[communicatingNpcName].pageObjects.Count > 0)
			bookPageNavigator.NavigateToPage(chapterObjectPerNpc[communicatingNpcName].pageObjects.Count - 1);


		string investigationSubjectKey = clickedInvestigationKey + investigationButtonTextParse;   // C_Knife_text, A_Details_text, ...
		CreateDialogueWith(investigationSubjectKey);

		notebookInfo.RecordAskedInvestigationSubject(communicatingNpcName, GetInvestigationSubject(clickedInvestigationKey));
		DeActiveQuestionButton(clickedInvestigationKey);

		EnableCommunication();


		if (GetInvestigationSubject(clickedInvestigationKey) == "Details") PlayDetailInvestigationVideo(communicatingNpcName);

	}


	// 나중에는 중요 문장은 따로 페이지 저장하기
	void OnAskedInvestigationButtonClicked(string clickedInvestigationRecordKey)
	{
		if (GetInvestigationSubject(clickedInvestigationRecordKey) == "Details")
		{
			PlayDetailInvestigationVideo(communicatingNpcName);
			return;
		}


		if (bookChapterNavigator.NavigatingChapterName != communicatingNpcName)
			bookChapterNavigator.NavigateNpcChapterTo(communicatingNpcName);


		var communicatingNpcPages = notebookInfo.ChapterRecordPerNpc[communicatingNpcName].pages;
		string investigationSubjectKey = clickedInvestigationRecordKey + investigationButtonTextParse;
		for (int pageNum = 0; pageNum < communicatingNpcPages.Count; pageNum++)
		{
			if (communicatingNpcPages[pageNum].keysPerPage.Contains(investigationSubjectKey))
			{
				bookPageNavigator.NavigateToPage(pageNum);

				var textObj = chapterObjectPerNpc[communicatingNpcName].pageObjects[pageNum].transform.transform.Find(investigationSubjectKey);
				StartCoroutine(BlinkTextForWhile(textObj, 3.0f));
			}
		}
		
	}


	void PostprocessDecisionEvent()
	{
		if (questionKeys != null) questionKeys.Clear();

		if (investigationEventController == null) return;
		investigationEventController.DestroyDecisionPanel();
		//investigationEventController.OnDecisionButtonClicked -= OnUnAskedInvestigationKeyClicked;
		investigationEventController.OnDecisionButtonClicked -= OnInvestigationButtonClicked;
	}


	void DeActiveQuestionButton(string buttonName)
	{
		//investigationEventController.SetButtonInteractable(buttonName, false);
		investigationEventController.ChangeButtonColor(buttonName, deActivatedColor);
	}


	void PlayDetailInvestigationVideo(string npcName)
	{
		GameModeManager.SetGameMode(GameMode.Media);   // videoplayer로 옮길지 말지 초고민 흠 -> 안 옮기는 게 나음, 오히려 컨트롤 어려워짐
		detailVideoPlayController.PlayVideoWithKey(npcName);
	}

	void OnDetailVideoPlayFinished()
	{
		isCommunicable = true;
		GameModeManager.SetGameMode(GameMode.InGame);
	}


	IEnumerator BlinkTextForWhile(Transform targetTextObj, float blinkTime)
	{
		var blinkTextComponent = targetTextObj.gameObject.GetComponent<BlinkText>() ?? targetTextObj.gameObject.AddComponent<BlinkText>();
		blinkTextComponent.IsKeepBlink = true;

		yield return new WaitForSecondsRealtime(blinkTime);

		if (blinkTextComponent) blinkTextComponent.IsKeepBlink = false;
	}


	#endregion


	#region Pre-Processing Dialogue

	
	Vector2 isNpcDataLoaded;


	char currentPhase;

	void LoadCommunicatingNpcData(Transform npc)
	{
		isNpcDataLoaded = Vector2.zero;


		communicatingNpcName = npc.GetComponent<InteractiveEntityInfo>().NpcInfo.NpcName;
		communicatingNpcData = dialogueInfo.NpcDialogueDictionary[communicatingNpcName];
		currentPhase = (PhaseChecker.GetCurrentPhase() <= 'B' ? 'A' : 'C');


		string latestLoggedKey = notebookInfo.GetLatestKeyOf(communicatingNpcName);
		latestWirttenDialogueKey = latestLoggedKey;


		communicatingNpcAudioGetter = new(this);
		StartCoroutine(LoadCommunicatingNpcAudio(communicatingNpcName, currentPhase.ToString()));


		LoadNpcBlenderShaper(npc);
		LoadAudioSourceController(npc);
		

		// if log exist -> 기존의 제일 최근 패널 activeTrue, 아니라면 새 패널 만들기.
		bool isNpcLogExist = chapterObjectPerNpc.ContainsKey(communicatingNpcName);
		if (isNpcLogExist)   chapterObjectPerNpc[communicatingNpcName].pageObjects[^1].SetActive(true);  // 챕터의 가장 최근 페이지 켜기
		else						   CreateCommunitingNpcChapter();																			   // 챕터 하나 만들기
		chapterObjectPerNpc[communicatingNpcName].chapterObj.SetActive(true);                                         // 챕터 켜기


		CreateInvestigationEventForFirstTime(isTransparent: true);

		LoadDialogueNavigators();


		isCommunicable = false;

		isNpcDataLoaded[0] = 1;
		if (isNpcDataLoaded[1] == 1 && isNotebookObjReady)
		{
			NotebookInfo.OnNotebookHoldReady();
			isNpcDataLoaded = Vector2.zero;
			isNotebookObjReady = false;
		}

	}



	IEnumerator LoadCommunicatingNpcAudio(string npcName, string phase)
	{
		// TODO: 추후 정리
		// 물어보지 않은 질문의 오디오만 골라서 빼오는 건 ㅇㄸ?
		// Ex) A.주제, C주제, ...
		yield return StartCoroutine(communicatingNpcAudioGetter.LoadNpcPhaseAudio(npcName, "A"));
		if (phase == "C")
			yield return StartCoroutine(communicatingNpcAudioGetter.LoadNpcPhaseAudio(npcName, "C"));


		isNpcDataLoaded[1] = 1;
		if (isNpcDataLoaded[0] == 1 && isNotebookObjReady)
		{
			NotebookInfo.OnNotebookHoldReady();
			isNpcDataLoaded = Vector2.zero;
			isNotebookObjReady = false;
		}

		isCommunicable = false;

	}



	bool isNotebookObjReady;
	void PreprocessNotebookObjects(Transform holdingNotebook, PlaceInfo placeInfo)
	{
		if (holdingNotebook.TryGetComponent<NotebookController>(out var notebookController));
		else Debug.LogWarning("[NULL] Notebook Controller Not Attatched to holdingNotebook!");

		this.placeInfo = placeInfo;
		notebookInfo = notebookController.GetNotebookInfo(placeInfo);
		//Debug.Log(placeInfo + " " + notebookInfo);
		notebookCanvas = notebookController.NotebookCanvas;

		pageNavigationRecordOnChapter = new();
		chapterObjectPerNpc = new();


		PreprocessBookUtility(holdingNotebook);
		LoadLoggedNotebookPages();

		isNotebookObjReady = true;
		if (isNpcDataLoaded == Vector2.one)
		{
			NotebookInfo.OnNotebookHoldReady();
			isNpcDataLoaded = Vector2.zero;
			isNotebookObjReady = false;
		}
	}

	/// <summary> 이전 Dialogue Log 전체 불러오기: Only Read </summary>
	void LoadLoggedNotebookPages()
	{
		if (notebookInfo.ChapterRecordPerNpc == null || notebookInfo.ChapterRecordPerNpc.Count == 0) return;

		List<string> communicatedNpcs = notebookInfo.ChapterRecordPerNpc.Keys;

		// Per NPC Chapter
		foreach (var communicatedNpc in communicatedNpcs)
		{
			var npcCommunicationData = dialogueInfo.NpcDialogueDictionary[communicatedNpc];
			var npcDialoguePanels = new List<GameObject>();
			var npcFolder = Instantiate(panelFolderPrefab, notebookCanvas, false);

			npcFolder.name = communicatedNpc;
			chapterObjectPerNpc[communicatedNpc] = new ChapterObject();


			// Per Page in NPC Chapter
			var pagesInChapterRecord = notebookInfo.ChapterRecordPerNpc[communicatedNpc].pages;
			for (int pageNum = 0; pageNum < pagesInChapterRecord.Count; pageNum++)
			{
				var eachPage = pagesInChapterRecord[pageNum];
				var dialoguePanel = Instantiate(npcDialoguePanelPrefab, npcFolder.transform, false);


				// Per Text in Page Of NPC Chapter
				var prevKey = "";
				foreach (var writtenKey in eachPage.keysPerPage)
				{
					bool isSpacerNeeded = (prevKey != "" && IsSpeakerChanged(prevKey, writtenKey, communicatedNpc));
					if (isSpacerNeeded) Instantiate(spacerTextPrefab, dialoguePanel.transform, false);

					var textObj = Instantiate(GetTextPrefab(communicatedNpc, writtenKey), dialoguePanel.transform, false);
					textObj.name = writtenKey;


					string newText;
					if (writtenKey.EndsWith(investigationButtonTextParse))
						newText = dialogueInfo.CommonDialogueDictionary["InvestigationEvent"][writtenKey][dialogueInfo.language];
					else
						newText = npcCommunicationData[writtenKey].dialogueByLanguage[dialogueInfo.language];

					textObj.GetComponentInChildren<TextMeshProUGUI>().text = newText;
					if (writtenKey.EndsWith(investigationButtonTextParse)) textObj.GetComponent<TextMeshProUGUI>().color = Color.yellow;



					// For Text Highlighting
					if (bookHighlighter != null)
					{
						bookHighlighter.LinkHighlightRecieverTo(textObj.transform, communicatedNpc);
						LinkCommunicationLimiterTo(textObj.transform);
					}


					prevKey = writtenKey;
				}


				if (pageNum != pagesInChapterRecord.Count - 1)
					dialoguePanel.SetActive(false);
				else
					pageNavigationRecordOnChapter[communicatedNpc] = pageNum;


				npcDialoguePanels.Add(dialoguePanel);
			}


			chapterObjectPerNpc[communicatedNpc].chapterObj = npcFolder;
			chapterObjectPerNpc[communicatedNpc].pageObjects = npcDialoguePanels;
			npcFolder.SetActive(false);
		}
	}



	#endregion

	#region Post-Processing Dialogue


	void PostprocessNpcDialogue()
	{
		isCommunicable = false;
		isBookChapterNavigatable = false;

		PostprocessDecisionEvent();
		PostprocessBookUtility();
		PostprocessTalkUtility();

		RecordLatestKeyOfCommunicatingNpc();
	}


	// 이젠 필요없고 걍 다 추가해주면 됑
	
	void RecordLatestKeyOfCommunicatingNpc()
	{
		bool isUnUpdatableKey = NpcDialogueKeyChecker.IsErrorKey(latestWirttenDialogueKey);
					//|| NpcDialogueKeyChecker.IsDecisionKey(latestWirttenDialogueKey) ||
				//								 (NpcDialogueKeyChecker.IsResultKeyOfDecision(latestWirttenDialogueKey) && !NpcDialogueKeyChecker.IsAnswerKey(latestWirttenDialogueKey));
		if (!isUnUpdatableKey)
			notebookInfo.UpdateLatestKeyOf(communicatingNpcName, latestWirttenDialogueKey);
	}


	#endregion


	GameObject GetTextPrefab(string npcSheet, string key)
	{
		if (key.EndsWith(investigationButtonTextParse))
		{
			return playerTextPrefab;
		}
		else
		{
			if (dialogueInfo.NpcDialogueDictionary[npcSheet][key].characterCode == CharacterCode.Player)
				return playerTextPrefab;
			else
				return npcTextPrefab;
		}
	}




	/// <summary> Get Only the subject of question key (A_Info -> Info, C_Knife_0 -> Knife) </summary>
	string GetInvestigationSubject(string investigationKey)
	{
		return investigationKey.Split('_')[1];
	}


	/// <summary>  해당 페이즈에서, 취조가 다 진행됐는지 반환  </summary>
	bool IsQuestionAllAsked(string npcName, char phase)
	{
		if (phase == 'A' || phase == 'C')
		{
			if (notebookInfo.AskedQuestionSubjectPerNpc == null) return false;
			if (!notebookInfo.AskedQuestionSubjectPerNpc.ContainsKey(npcName)) return false;
		}
		else
		{
			return true;
		}


		foreach (var aQuestion in DialogueInfo.basicInvestigationQuestions)
		{
			if (!notebookInfo.AskedQuestionSubjectPerNpc[npcName].questionList.Contains(aQuestion))
				return false;
		}

		if (phase == 'C')
		{
			foreach (var evidence in placeInfo.AllEvidencesInPlace)
			{
				if (!notebookInfo.AskedQuestionSubjectPerNpc[npcName].questionList.Contains(evidence.name))
					return false;
			}
		}


		return true;

	}



	#region Dialogue Key Related

	bool IsSpeakerChanged(string prevKey, string currentKey, string checkingNpcSheet)
	{
		//Debug.Log(prevKey + " -> " + currentKey);

		if (currentKey.Length == 0) return false;

		prevKey = (prevKey != "" ? prevKey : GetKeyOfLastTextChild());
		if (prevKey == "") return false;


		CharacterCode prevPlayerCode = (prevKey.EndsWith(investigationButtonTextParse) ? CharacterCode.Player :
																dialogueInfo.NpcDialogueDictionary[checkingNpcSheet][prevKey].characterCode);
		CharacterCode newPlayerCode = (currentKey.EndsWith(investigationButtonTextParse) ? CharacterCode.Player :
														        dialogueInfo.NpcDialogueDictionary[checkingNpcSheet][currentKey].characterCode);


		return (prevPlayerCode != newPlayerCode);
	}

	bool IsInvesitgationDialogueLeft()
	{
		return (latestWirttenDialogueKey != "" && NpcDialogueKeyGetter.GetNextKeyInNpcSheet(latestWirttenDialogueKey, communicatingNpcData) != null);
	}


	string GetDialogueText(string key)
	{
		if (key.EndsWith(investigationButtonTextParse))
			return (dialogueInfo.CommonDialogueDictionary["InvestigationEvent"][key][dialogueInfo.language]);

		else
			return (dialogueInfo.NpcDialogueDictionary[communicatingNpcName][key].dialogueByLanguage[dialogueInfo.language]);

	}

	




	/*
	/// <summary>  Return Error key if player hasn't cleared PE of npc  </summary>
	/// 
	string GetNewPhaseKeyByPEStatus(string lastKeyOfPrevPhase)
	{
		string nextPhase = (lastKeyOfPrevPhase.Length == 0 ? "A" : NpcDialogueKeyGetter.GetDialoguePhaseOfKey(NpcDialogueKeyGetter.GetNextKeyByIndex(lastKeyOfPrevPhase, communicatingNpcData)));
		bool isPEProceeded = (nextPhase == "A" ? true : communicatingNpc.PEProgressPerPhase[nextPhase]);

		if (isPEProceeded)
			return (NpcDialogueKeyChecker.IsKeyValid(nextPhase + "_1", communicatingNpcData) ? (nextPhase + "_1") : (nextPhase + "_1_answer"));
		else
			return (nextPhase + DialogueInfo.phaseErrorParse);
	}*/


	#endregion

	#region UI Object Related


	#region Create, Destroy: Panel & Text

	void CreateDialogueUI(string key)
	{
		if (!IsTextWrittenRightBefore(key))
		{
			CreateDialoguePanelOnOverflow();
			CreateDialogueTextOnPanel(key);
		}

	}

	void CreateDialoguePanelOnOverflow()
	{
		bool isFirstCommunication = chapterObjectPerNpc[communicatingNpcName].pageObjects.Count == 0;
		if (isFirstCommunication) CreateNewDialoguePanel();

		if (IsLastPanelFull())
		{
			GameObject previousPanel = chapterObjectPerNpc[communicatingNpcName].pageObjects[^1];
			previousPanel.SetActive(false);

			CreateNewDialoguePanel();

			OnNewPageCreated();
		}
	}

	void CreateNewDialoguePanel()
	{
		var newPanel = Instantiate(npcDialoguePanelPrefab, chapterObjectPerNpc[communicatingNpcName].chapterObj.transform, false);
		chapterObjectPerNpc[communicatingNpcName].pageObjects.Add(newPanel);

		notebookInfo.RecordNewPageOf(communicatingNpcName);
	}



	void CreateDialogueTextOnPanel(string newKey)
	{
		//Debug.Log("create: " + newKey);

		var latestDialoguePanel = chapterObjectPerNpc[communicatingNpcName].pageObjects[^1];


		if (IsSpacerNeeded(newKey))   Instantiate(spacerTextPrefab, latestDialoguePanel.transform, false);

		var textObj = Instantiate(GetTextPrefab(communicatingNpcName, newKey), latestDialoguePanel.transform, false);
		textObj.name = newKey;


		string newText;
		if (newKey.EndsWith(investigationButtonTextParse))
			newText = dialogueInfo.CommonDialogueDictionary["InvestigationEvent"][newKey][dialogueInfo.language];
		else
			newText = communicatingNpcData[newKey].dialogueByLanguage[dialogueInfo.language];
		textObj.GetComponent<TextMeshProUGUI>().text = newText;

		if (newKey.EndsWith(investigationButtonTextParse) || NpcDialogueKeyChecker.IsErrorKey(newKey))
			textObj.GetComponent<TextMeshProUGUI>().color = Color.yellow;

		


		if (bookHighlighter != null)
		{
			bookHighlighter.LinkHighlightRecieverTo(textObj.transform, communicatingNpcName);
			LinkCommunicationLimiterTo(textObj.transform);
		}



		if(! NpcDialogueKeyChecker.IsErrorKey(newKey))
			notebookInfo.RecordNewTextOf(communicatingNpcName, newKey);
		/*
		if (!NpcDialogueKeyChecker.IsResultKeyOfDecision(newKey) && !NpcDialogueKeyChecker.IsDecisionKey(newKey))
		{
			notebookInfo.RecordNewTextOf(communicatingNpcName, newKey);
		}
		else
		if (NpcDialogueKeyChecker.IsResultKeyOfDecision(newKey) && NpcDialogueKeyChecker.IsAnswerKey(newKey))
		{
			notebookInfo.RecordNewTextOf(communicatingNpcName, latestWirttenDialogueKey);
			notebookInfo.RecordNewTextOf(communicatingNpcName, newKey);
		}
		*/

		latestWirttenDialogueKey = newKey;
	}


	void CreateCommunitingNpcChapter()
	{
		chapterObjectPerNpc[communicatingNpcName] = new ChapterObject();

		var npcChapterObj = Instantiate(panelFolderPrefab, notebookCanvas, false);
		npcChapterObj.name = communicatingNpcName;
		chapterObjectPerNpc[communicatingNpcName].chapterObj = npcChapterObj;

		OnNewChapterCreated();
	}


	#endregion

	#region Check: Panel & Text


	readonly float panelBottomPos = -1.4f;

	bool IsLastPanelFull()
	{
		var usingPanel = chapterObjectPerNpc[communicatingNpcName].pageObjects[^1].transform;
		if (usingPanel.transform.childCount == 0) return false;

		var lastText = usingPanel.GetChild(usingPanel.transform.childCount - 1);

		float yPos = lastText.GetComponent<RectTransform>().anchoredPosition.y;


		if (yPos < panelBottomPos) return true;
		else return false;
	}


	bool IsSpacerNeeded(string key)
	{
		if (IsPanelEmpty(chapterObjectPerNpc[communicatingNpcName].pageObjects[^1].transform))
			return false;

		return IsSpeakerChanged(latestWirttenDialogueKey, key, communicatingNpcName) || NpcDialogueKeyChecker.IsErrorKey(key); 
		//return IsSpeakerChanged(latestWirttenDialogueKey, key, communicatingNpcName);
	}



	bool IsTextWrittenRightBefore(string key)
	{
		var prevKey = (latestWirttenDialogueKey == "" ? GetKeyOfLastTextChild() : latestWirttenDialogueKey);

		if (prevKey == "") return false;
		if (prevKey == key) return true;

		return (GetDialogueText(prevKey) == GetDialogueText(key));
	}


	bool IsPanelEmpty(Transform panelTransform)
	{
		return (panelTransform.childCount == 0);
	}


	bool IsNavigatingFinalPageOfCommunicatingNpc()
	{
		Debug.Log("Chapter: " + isChapterCommunicatingNpc + " / Page: " + isPageLastOfCommunicatingNpc);


		return isChapterCommunicatingNpc && isPageLastOfCommunicatingNpc;
	}


	#endregion

	#region Get: Panel & Text

	/// <summary>  Returns null if there is no panel Or child  </summary>
	Transform GetLastTextChildInPanel()
	{
		if (!chapterObjectPerNpc.ContainsKey(communicatingNpcName)) return null;

		int latestUsedPanelIndex = chapterObjectPerNpc[communicatingNpcName].pageObjects.Count - 1;
		if (latestUsedPanelIndex < 0) return null;

		var latestUsedPanel = chapterObjectPerNpc[communicatingNpcName].pageObjects[^1].transform;
		if (!IsPanelEmpty(latestUsedPanel)) return latestUsedPanel.GetChild(latestUsedPanel.childCount - 1);
		else
		{
			if (latestUsedPanelIndex == 0) return null;

			latestUsedPanel = chapterObjectPerNpc[communicatingNpcName].pageObjects[^2].transform;
			return latestUsedPanel.GetChild(latestUsedPanel.childCount - 1);
		}
	}


	/// <summary>  Return "" if there is no text  </summary>
	string GetKeyOfLastTextChild()
	{
		Transform lastTextInPanel = GetLastTextChildInPanel();
		if (lastTextInPanel == null) return "";

		return lastTextInPanel.name;
	}


	/// <summary> Need to Update Local MousePos if IsSentenceHighlightable  </summary>
	Vector2 GetUILocaledMousePos(Transform targetUI)
	{
		var rectTransform = targetUI.GetComponent<RectTransform>();
		var mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, Camera.main, out var localMousePos);

		return localMousePos;
	}


	#endregion

	#endregion


	// button, indexer pointer exit/enter 시 isCommunication 변화 관리
	void DisableCommunication()
	{
		//if ( isDecisionEventOn ) return;
		if (IsInvesitgationDialogueLeft())   return;

		if ( isCommunicable ) isCommunicable = false;

	}
	void EnableCommunication()
	{
		//if ( isDecisionEventOn ) return;
		//if (IsInvesitgationDialogueLeft())   return;

		// npc챕터의 마지막 페이지를 탐색 중일 때에만 isCommuncable해야하기 때문에 페이지 확인 // -> 이제 탐색은 대화 끝났을 때에만 할 수 있기 때문에 굳이 필요 X
		//if (IsNavigatingFinalPageOfCommunicatingNpc() && !isCommunicable ) isCommunicable = true;
		if ( ! isCommunicable) isCommunicable = true;

		//Debug.Log("Communcable: " + isCommunicable);
	}


	#endregion



	#region Talk Utility

	NpcMouthShapeController communicatingNpcMouthShaper;
	AudioSourceController npcAudioSourceController;
	AudioSourceController playerAudioSouceController;
	float mouseDuration = 0.3f;
	// 0.5-> 좀 느린 감 있음

	NpcAudioAssetGetter communicatingNpcAudioGetter;
	bool isNpcTalking = false;


	void CreateDialogueTalk(string key)
	{
		//if (key.EndsWith(investigationButtonTextParse)) return;
		//if ( communicatingNpcData[key].characterCode == CharacterCode.Player || communicatingNpcData[key].pronounce == Pronounce.None)  // 주의: Sheet에서 꼭 삽입할 것
		//{
		//	CloseCommunicatingNpcMouse();
		//	return;
		//}


		if(communicatingNpcAudioGetter.AudioKeyContainer.ContainsKey(key))
		{
			if (communicatingNpcData[key].characterCode == CharacterCode.Npc)
			{
				PlayCommunicatingNpcAudio(key);
				OpenCommunicatingNpcMouse(communicatingNpcData[key].pronounce);
			}
			else
			{
				PlayPlayerAudio(key);
				CloseCommunicatingNpcMouse();
			}
		}
		else
		{
			CloseCommunicatingNpcMouse();
			return;
		}

	}


	void PlayCommunicatingNpcAudio(string key)
	{
		//if (communicatingNpcAudioGetter.AudioKeyContainer.ContainsKey(key))
		//{
			playerAudioSouceController.StopAudioClip();
			
			npcAudioSourceController.PlayAudioClip(communicatingNpcAudioGetter.AudioKeyContainer[key]);
		//}
	}
	void PlayPlayerAudio(string key)
	{
		npcAudioSourceController.StopAudioClip();

		playerAudioSouceController.PlayAudioClip(communicatingNpcAudioGetter.AudioKeyContainer[key]);
	}


	void OpenCommunicatingNpcMouse(Pronounce pronounce)
	{
		if (communicatingNpcMouthShaper == null) return;

		if (!isNpcTalking) isNpcTalking = true;

		if(isNpcTalking) StartCoroutine(OpenCommunicatingNpcMouseInLoop(pronounce));
	}

	IEnumerator OpenCommunicatingNpcMouseInLoop(Pronounce firstPronounce)
	{
		Pronounce pronounce = firstPronounce;
		Pronounce[] pronounces = (Pronounce[])System.Enum.GetValues(typeof(Pronounce));
		mouseDuration = Random.Range(0.1f, 0.3f);
		int i = 0;

		while (isNpcTalking)
		{
			yield return StartCoroutine(communicatingNpcMouthShaper.OpenMouthSmoothly(pronounce, 100, mouseDuration));
			//Debug.Log("Open Done");

			Pronounce newPronounce = RandomizeEnumValues(pronounces);
			while (newPronounce == pronounce || newPronounce == Pronounce.None)   newPronounce = RandomizeEnumValues(pronounces);
			pronounce = newPronounce;
			mouseDuration = Random.Range(0.1f, 0.3f);

			//Debug.Log("Talk : " + i + " / " + communicatingNpcMouthShaper.PrevPronounce + " -> " + newPronounce);
			i++;

			yield return null;
		}
	}


	// TODO: 딴 데로 빼기. enum 모아서 폴더에 넣든
	T RandomizeEnumValues<T>(T[] enumArray) where T: System.Enum
	{
		int randomIndex = Random.Range(0, enumArray.Length - 1);

		return (enumArray[randomIndex]);
	}



	void CloseCommunicatingNpcMouse()
	{
		if (isNpcTalking) isNpcTalking = false;
		if (!communicatingNpcMouthShaper.IsMouthOpen && communicatingNpcMouthShaper == null) return;

		if(communicatingNpcMouthShaper.PrevPronounce != Pronounce.None)
			StartCoroutine(communicatingNpcMouthShaper.OpenMouthSmoothly(communicatingNpcMouthShaper.PrevPronounce, 0, mouseDuration));
	}



	void LoadAudioSourceController(Transform communicatingNpc)
	{
		var audioSource = communicatingNpc.GetComponent<AudioSource>();
		npcAudioSourceController = new AudioSourceController(audioSource);

		playerAudioSouceController = new AudioSourceController(playerAuidoSource);
	}

	void LoadNpcBlenderShaper(Transform npcTransform)
	{
		var communicatingNpcBlenderShaper = npcTransform.GetComponent<BlenderShapeController>();

		if(communicatingNpcBlenderShaper)
			communicatingNpcMouthShaper = new NpcMouthShapeController(communicatingNpcBlenderShaper);
	}


	void PostprocessTalkUtility()
	{
		CloseCommunicatingNpcMouse();
	}


	#endregion


	#region Book Utility


	#region Navigation

	Dictionary<string, int> pageNavigationRecordOnChapter;

	bool isChapterCommunicatingNpc;
	bool isPageLastOfCommunicatingNpc;

	static readonly Color greenColor = new(0.23f, 0.74f, 0.61f, 1);
	static readonly Color yellowColor = new(0.74f, 0.67f, 0.25f, 1);

	// TODO: 선택 이벤트 중이면 아예 눌리지 않게 하기
	void OnChapterFlipped(string previousChapterName, string newChapterName)
	{
		int navigatedPageRecord = pageNavigationRecordOnChapter[newChapterName];
		//("Record: " + navigatedPageRecord);
		bookPageNavigator.RecieveNewChapter(chapterObjectPerNpc[newChapterName].pageObjects, navigatedPageRecord);

		bookChapterIndexerCreator.BookChapterIndexerDic[previousChapterName].ChangeColorTo(greenColor);
		bookChapterIndexerCreator.BookChapterIndexerDic[newChapterName].ChangeColorTo(yellowColor);

		isChapterCommunicatingNpc = (newChapterName == communicatingNpcName ? true : false);

		isCommunicable = IsNavigatingFinalPageOfCommunicatingNpc();
	}

	// TODO: 선택 이벤트 중이면 아예 눌리지 않게 하기
	void OnPageFlipped(List<GameObject> navigatingBookChapter, int navigatingPageIndex)
	{
		pageNavigationRecordOnChapter[bookChapterNavigator.NavigatingChapterName] = navigatingPageIndex;

		int lastPageOfCommunicatingNpc = chapterObjectPerNpc[communicatingNpcName].pageObjects.Count - 1;
		if (lastPageOfCommunicatingNpc < 0) lastPageOfCommunicatingNpc = 0;

		isPageLastOfCommunicatingNpc = (navigatingPageIndex == lastPageOfCommunicatingNpc ? true : false);

		isCommunicable = IsNavigatingFinalPageOfCommunicatingNpc();
	}

	void OnIndexerPointerEnter() 
	{
		DisableCommunication(); 
	}
	void OnIndexerPointerExit() 
	{
		EnableCommunication(); 
	}



	void InitializePageNavigatingIndex(string newChapterName)
	{
		pageNavigationRecordOnChapter[newChapterName] = 0;
	}

	void UpdatePageNavigatingIndex()
	{
		pageNavigationRecordOnChapter[communicatingNpcName] += 1;

		bookPageNavigator.NavigatingPageIndex = pageNavigationRecordOnChapter[communicatingNpcName];
	}



	void LoadDialogueNavigators()
	{
		// A-1. Chapter Indexer Creator
		foreach (var npcName in chapterObjectPerNpc.Keys)
		{
			if(npcName == communicatingNpcName)
				bookChapterIndexerCreator.CreateNpcIndexer(npcName, yellowColor);
			else
			{
				if (notebookInfo.ChapterRecordPerNpc.ContainsKey(npcName) && notebookInfo.ChapterRecordPerNpc[npcName].pages[0].keysPerPage.Count != 0)
					bookChapterIndexerCreator.CreateNpcIndexer(npcName, greenColor);
			}
		}

		// A-2. Chapter Navigator
		var chapterObjList = new List<GameObject>();
		foreach (var chapterObj in chapterObjectPerNpc.Values) chapterObjList.Add(chapterObj.chapterObj);
		bookChapterNavigator = new BookChapterNavigator(chapterObjList, communicatingNpcName);
		bookChapterNavigator.OnChapterFlipped += OnChapterFlipped;
		bookChapterNavigator.OnIndexerPointerEnter += OnIndexerPointerEnter;
		bookChapterNavigator.OnIndexerPointerExit += OnIndexerPointerExit;

		isChapterCommunicatingNpc = true;


		// B. Page Navigator
		bookPageNavigator.RecieveNewChapter(chapterObjectPerNpc[communicatingNpcName].pageObjects,
																		   pageNavigationRecordOnChapter[communicatingNpcName]);
		isPageLastOfCommunicatingNpc = true;


		isBookChapterNavigatable = true;
	}




	#endregion


	#region Highlight

	void UpdateDialogueWithHighlightedText()
	{
		if (bookHighlighter.MarkedKeyTextPerSheet.Count != 0)
		{
			//if (NpcDialogueKeyChecker.IsDecisionKey(latestWirttenDialogueKey))
				//bookHighlighter.RemoveKeyOfHighlightTextRecord(communicatingNpcName, latestWirttenDialogueKey);
			//else if (NpcDialogueKeyChecker.IsResultKeyOfDecision(latestWirttenDialogueKey) && NpcDialogueKeyChecker.IsErrorKey(latestWirttenDialogueKey))
			//{
				//bookHighlighter.RemoveKeyOfHighlightTextRecord(communicatingNpcName, latestWirttenDialogueKey);
				//bookHighlighter.RemoveKeyOfHighlightTextRecord(communicatingNpcName, NpcDialogueKeyGetter.GetDecisionKeyOfDecisionResult(latestWirttenDialogueKey));
			//}
		}
		foreach (var dialogueChangedNpc in bookHighlighter.MarkedKeyTextPerSheet)
		{
			string npcName = dialogueChangedNpc.Key;
			var changedDialogueKeyTexts = dialogueChangedNpc.Value;

			dialogueInfo.UpdateKeyTextsOfSheet(npcName, changedDialogueKeyTexts);
		}
	}


	#endregion


	void PreprocessBookUtility(Transform holdingNotebook)
	{
		bookChapterIndexerCreator = holdingNotebook.GetComponent<BookChapterIndexerCreator>();
		bookHighlighter = notebookInfo.BookHighlighter;

		if (holdingNotebook.TryGetComponent<NotebookController>(out var notebookController)) ;
		else Debug.LogWarning("[NULL] Notebook Controller Not Attatched to holdingNotebook!");

		LinkCommunicationLimiterTo(notebookController.LeftButtonObj.transform);
		LinkCommunicationLimiterTo(notebookController.RightButtonObj.transform);
		bookPageNavigator = new BookPageNavigator(notebookController.LeftButtonObj, notebookController.RightButtonObj);
		bookPageNavigator.OnPageFlipped += OnPageFlipped;
	}

	void PostprocessBookUtility()
	{
		bookHighlighter.Postprocess(UpdateDialogueWithHighlightedText);
		UnlinkBookNavigatorDelegate();
	}



	void UnlinkBookNavigatorDelegate()
	{
		bookPageNavigator.OnPageFlipped -= OnPageFlipped;
		bookChapterNavigator.OnChapterFlipped -= OnChapterFlipped;
		bookChapterNavigator.OnIndexerPointerEnter -= OnIndexerPointerEnter;
		bookChapterNavigator.OnIndexerPointerExit -= OnIndexerPointerExit;
	}


	#endregion


	#region Link Event Trigger: Button, Text


	void LinkCommunicationLimiterTo(Transform transform)
	{
		var eventTrigger = transform.GetComponent<EventTrigger>() ?? transform.gameObject.AddComponent<EventTrigger>();

		EventTriggerLinker.LinkEventTriggerTo<PointerEventData>(eventTrigger, EventTriggerType.PointerEnter, DisableCommunicationOnPointerEnter);
		EventTriggerLinker.LinkEventTriggerTo<PointerEventData>(eventTrigger, EventTriggerType.PointerExit, EnableCommunicationOnPointerExit);
	}

	void DisableCommunicationOnPointerEnter(PointerEventData data) { DisableCommunication(); }
	void EnableCommunicationOnPointerExit(PointerEventData data) { EnableCommunication(); }




	#endregion



}
