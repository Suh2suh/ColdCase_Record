using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DialogueInfo", menuName = "ScriptableObjects/Informations/DialogueInfo", order = 1)]
public class DialogueInfo : ScriptableObject
{

	public Language language = Language.Korean;
	


	public static List<string> CommonDialogueSheetNames = new() { "Setting", "ItemName", "ItemExplanation", "Narration",
																  "Book", "InvestigationEvent", "WalkieTalkie"};

	[SerializeField] UDictionary<string, UDictionary<string, UDictionary<Language, string>>> commonDialogueDictionary;



	/// <summary> <b>UNUpdateble</b>: Only for saving, refreshing changed npcDilaogueDictionary </summary>
	[SerializeField] UDictionary<string, UDictionary<string, CommunicationData>> cleanNpcDialogueDictionary;


	// 
	/// <summary> <b>Updatable</b>: rich text(such as mark, b...) can be pushed </summary>
	[SerializeField] UDictionary<string, UDictionary<string, CommunicationData>> npcDialogueDictionary;






	#region Getters

	public UDictionary<string, UDictionary<string, UDictionary<Language, string>>> CommonDialogueDictionary { get => commonDialogueDictionary; }
	public UDictionary<string, UDictionary<string, CommunicationData>> CleanNpcDialogueDictionary { get => cleanNpcDialogueDictionary; }
	public UDictionary<string, UDictionary<string, CommunicationData>> NpcDialogueDictionary { get => npcDialogueDictionary; set => npcDialogueDictionary = value; }



	#endregion



	#region NPC Dialogue Information

	public Transform holdingNotebook;

	public static System.Action<Transform> OnNpcClicked;
	public static System.Action OnNpcDialogueStart;
	public static System.Action OnNpcDialogueEnd;

	//readonly public static string answerParse = "_answer";
	readonly public static string errorParse = "_Error";
	//readonly public static string selectErrorParse = "_error";

	readonly public static List<string> basicInvestigationQuestions
	= new() { "Info", "Visit","Relationship", "Details" };




	#region Old Code - Npc Dialogue

	/*

	/// <summary>  NPC Touch �� True, NPC ��ȭ ������ �� False  </summary>
	public bool isDialogueNeed = false;
	/// <summary>  currentTalking NPC�� ���Դ���. ��ȭ �����ϴ� ������ True.  </summary>
	public bool isNpcValid = false;

	string playerDialoguePhase;
	public string PlayerDialoguePhase
	{
		get => playerDialoguePhase;
	}
	public void UpdatePlayerDialoguePhase()
	{
		char nextPhase = playerDialoguePhase[0];
		nextPhase++;
		playerDialoguePhase = nextPhase.ToString();

		Debug.Log("Player Phase Update: " + playerDialoguePhase);
	}
	public void UpdatePlayerChoiceRecord(DialogueSheet npcName)
	{
		var latestNpcPhase = GetLatestDialoguePhaseOf(npcName)[0];
		latestNpcPhase++;


		if (!playerChoiceRecord[npcName].Keys.Contains(latestNpcPhase.ToString()))
		{
			playerChoiceRecord[npcName].Add(latestNpcPhase.ToString(), "");
			Debug.Log("Npc Phase Update: " + latestNpcPhase);
		}
	}


	public Transform CurrentTalkingNpcTransform;


	DialogueSheet currentTalkingNpc;
	public DialogueSheet CurrentTalkingNpc
	{
		get => currentTalkingNpc;
		set
		{
			if (value != DialogueSheet.Narration)
			{
				currentTalkingNpc = value;
				isDialogueNeed = true;
				isNpcValid = true;
			}
			else
			{
				Debug.LogError("[Hint Object Component] ]NPC Type Setting ERROR!");
			}
		}
	}


	UDictionary<DialogueSheet, UDictionary<string, string>> correctChoiceTree = new();
	public UDictionary<DialogueSheet, UDictionary<string, string>> CorrectChoiceTree
	{
		get => correctChoiceTree;
	}

	public void initializeCorrectChoiceTree()
	{
		if (!correctChoiceTree.Keys.Contains(DialogueSheet.Prank))
			correctChoiceTree.Add(DialogueSheet.Prank, new UDictionary<string, string>() { { "A", "-a-b" }, { "B", "-b" } });
	}


	// [Prank] : { APhase : -a-b-c..., BPhase : -a- b-b... }
	UDictionary<DialogueSheet, UDictionary<string, string>> playerChoiceRecord;
	public UDictionary<DialogueSheet, UDictionary<string, string>> PlayerChoiceRecord
	{
		get => playerChoiceRecord;
	}

	/// <summary>  �÷��̾ �б��� ��ư Ŭ������ �� ����.  </summary>
	public void UpdatePlayerChoiceRecord(DialogueSheet npcName, string decisionPoint)
	{
		var LatestPhaseOfNpc = GetLatestDialoguePhaseOf(npcName);

		playerChoiceRecord[npcName][LatestPhaseOfNpc] += ("-" + decisionPoint);

		SetDirtyThis();
		Debug.Log("[ " + npcName.ToString() + " | Decision Point Update ]" + playerChoiceRecord[npcName][LatestPhaseOfNpc]);
	}
	public string GetLatestDialoguePhaseOf(DialogueSheet npcName)
	{
		return playerChoiceRecord[npcName].Keys[playerChoiceRecord[npcName].Count - 1];
	}
	public void ResetPlayerChoiceRecord(DialogueSheet npcName)
	{
		var LatestPhaseOfNpc = GetLatestDialoguePhaseOf(npcName);

		playerChoiceRecord[npcName][LatestPhaseOfNpc] = "";
	}


	public void ValidateNpcDialogueData()
	{
		if (playerChoiceRecord == null || playerChoiceRecord.Keys.Count == 0)
			InitializeNpcDialogueData();
	}

	public string GetLatestPlayerChoiceRecordOf(DialogueSheet npcName)
	{
		var LatestPhaseOfNpc = GetLatestDialoguePhaseOf(npcName);

		return playerChoiceRecord[npcName][LatestPhaseOfNpc];
	}

	public void InitializeNpcDialogueData()
	{
		playerDialoguePhase = "A";
		playerChoiceRecord = new();

		var aPhaseDicForInitialization = new UDictionary<string, string>() { { "A", "" } };

		// HMM: foreach�ιٲܱ����... ��
		playerChoiceRecord.Add(DialogueSheet.Prank, aPhaseDicForInitialization);
		playerChoiceRecord.Add(DialogueSheet.Miranda, aPhaseDicForInitialization);


		SetDirtyThis();
		Debug.Log("���� ���̾�α� ���� ������ ����... �ʱ�ȭ �Ϸ�");
	}

	public bool CheckPlayerPhaseMatchWith(DialogueSheet npcName)
	{
		//if(isNpcValid)
		//{
		return (playerDialoguePhase == GetLatestDialoguePhaseOf(npcName));
		//}

		//return false;
	}
		/// <summary>
	/// Only before build
	/// </summary>
	public void ResetNpcDialogueData()
	{
		//playerDialoguePhase = "";
		//playerChoiceRecord = null;
		//SetDirtyThis();

		InitializeNpcDialogueData();
	}

	*/




	#endregion


	#endregion


	#region Narrative Dialogue Information

	/// <summary>  Narration Obj�� TriggerOn�� true, �����̼� ���� �� False  </summary>
	public bool isNarationNeed = false;

	/// <summary> ���� �����̼� �����͸� �߸� �޾ƿ��� �ʵ��� ��������  </summary>
	public bool isNarrationValid = false;


	private NarrationData[] currentNarrationData;
	public NarrationData[] CurrentNarrationData
	{
		get => currentNarrationData;
	}


	public void SetNewNarrationData(NarrationData[] newNarrationData)
	{
		// ���� �ڸ� ����, �� �ڸ� ����
		currentNarrationData = newNarrationData;

		isNarrationValid = true;
	}


	#endregion


	#region WalkieTalkie Dialogue Information

	public static System.Action OnWalkieTalkieDialogueStart;
	public static System.Action OnWalkieTalkieDialogueEnd;


	#endregion



	public string GetTranslatedText(string sheetName, string key)
	{
		if (CommonDialogueSheetNames.Contains(sheetName))
			return commonDialogueDictionary[sheetName][key][language];
		else
			return NpcDialogueDictionary[sheetName][key].dialogueByLanguage[language];
	}



	public void UpdateKeyTextsOfSheet(string sheetName, Dictionary<string, string> keyTextPairs)
	{
		if (sheetName == "Setting" || sheetName == "Narration" || sheetName == "Item" || sheetName == "InvestigationEvent") return;

		if (sheetName == "Book")
		{
			foreach (var keyTextPair in keyTextPairs)
				commonDialogueDictionary[sheetName][keyTextPair.Key][language] = keyTextPair.Value;
		}
		else
		{
			foreach (var keyTextPair in keyTextPairs)
				if (NpcDialogueDictionary[sheetName].ContainsKey(keyTextPair.Key))
					npcDialogueDictionary[sheetName][keyTextPair.Key].dialogueByLanguage[language] = keyTextPair.Value;
		}

		SetDirtyThis();
	}



	void SetDirtyThis()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}



	// < ONLY BEFORE BUILD > -> ���Ŀ��� CSV ���� ���� �ٿ�ε��ؼ� ���� OR Google API�� ����, ���� ������ Update �Ұ��ϰ� �� ��
	#region ONLY BEFORE BUILD: ERASE LATER


	/// <summary> Npc�� �ƴ� CSV Sheet (Narration, Item...) </summary>
	public void UpdateCommonDialogueDictionary(string sheetName, UDictionary<string, UDictionary<Language, string>> sheetDialogue)
	{
		if (commonDialogueDictionary == null)
		{
			commonDialogueDictionary = new();
			Debug.Log("dialogue dictionary initialized: was null before");
		}

		commonDialogueDictionary[sheetName] = sheetDialogue;

		CustomCSVReader.PrintDialogueDic(sheetDialogue);

		SetDirtyThis();
	}

	/// <summary>  npcName == sheetName == ScriptableObjectName  </summary>
	public void UpdateNpcDialogueDictionary(string npcName, UDictionary<string, CommunicationData> sheetDialogue)
	{
		if (npcDialogueDictionary == null) npcDialogueDictionary = new();
		if (cleanNpcDialogueDictionary == null) cleanNpcDialogueDictionary = new();


		npcDialogueDictionary[npcName] = sheetDialogue;
		cleanNpcDialogueDictionary[npcName] = sheetDialogue;


		SetDirtyThis();
	}


	/// <summary>  Refresh Npc Dialogue to default: Remove Highlight Sections  </summary>
	/// TODO: Later, just clean highlightened npc not all.
	public void CleanNpcDialogue()
	{
		string jsonOnlyForCopy = Newtonsoft.Json.JsonConvert.SerializeObject(cleanNpcDialogueDictionary);
		npcDialogueDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<UDictionary<string, UDictionary<string, CommunicationData>>>(jsonOnlyForCopy);

		SetDirtyThis();
	}

	#endregion

}
