using System.Collections.Generic;
using UnityEngine;
using ColdCase.Dialogue.Book.Utility;


[CreateAssetMenu(fileName = "NotebookInfo", menuName = "ScriptableObjects/Informations/NotebookInfo", order = 1)]
public class NotebookInfo : ScriptableObject
{
	void SetDirtyThis()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}
	private void OnEnable()
	{
		//형광펜
		//if(isHighlightable) bookHighlighter = new BookHighlighter_Mark(highlightColor, highlightPadding);
		
		//마커
		if (isHighlightable) bookHighlighter = new BookHighlighter_Color(highlightColor);
	}



	[SerializeField] bool isHighlightable;
	[SerializeField] BookHighlighter bookHighlighter;
	[HideInInspector] public BookHighlighter BookHighlighter { get => bookHighlighter; }
	[SerializeField] Color highlightColor;
	[SerializeField] Vector4 highlightPadding;


	/// <summary>  delegate 호출 시, 최초 npc 다이얼로그 전체 초기화  </summary>
	public static System.Action<Transform, PlaceInfo> OnNotebookCreated;
	public static System.Action OnNotebookHoldReady;



	#region Question Record Per Npc


	/// <summary> npcName: [질문 주제1, 주제2, ...] </summary>
	[SerializeField] UDictionary<string, QuestionList> askedQuestionSubjectPerNpc;
	[HideInInspector] public UDictionary<string, QuestionList> AskedQuestionSubjectPerNpc { get => askedQuestionSubjectPerNpc; set => askedQuestionSubjectPerNpc = value; }

	/// <summary> npc에게 했던 질문 주제들 기억. </summary>
	public void RecordAskedInvestigationSubject(string npcName, string questionKey)
	{
		if (askedQuestionSubjectPerNpc == null) askedQuestionSubjectPerNpc = new();
		if ( ! askedQuestionSubjectPerNpc.ContainsKey(npcName)) askedQuestionSubjectPerNpc[npcName] = new();

		if ( ! askedQuestionSubjectPerNpc[npcName].questionList.Contains(questionKey))
			askedQuestionSubjectPerNpc[npcName].questionList.Add(questionKey);

		Debug.Log(npcName + " / " + questionKey + " is asked ");
	}
	// phase Check(npcName, phase) -> phase의 질문이 여기에 다 들어갔는지. 체크
	// 각 페이즈마다의 질문 -> A같은 경우는 대화 그 자체기 때문에 여기있으면 됨.
	// C -> 게임 시작 할 때, DetectiveDesk에서 쫙 불러오기




#endregion


#region Chapter Obj Record Per Npc


	[SerializeField] UDictionary<string, Chapter> chapterRecordPerNpc;
	public UDictionary<string, Chapter> ChapterRecordPerNpc { get => chapterRecordPerNpc; set => chapterRecordPerNpc = value; }



	public void RecordNewPageOf(string npcName)
	{
		if (chapterRecordPerNpc == null) chapterRecordPerNpc = new();
		if (!chapterRecordPerNpc.Keys.Contains(npcName)) chapterRecordPerNpc[npcName] = new Chapter();

		chapterRecordPerNpc[npcName].pages.Add(new Page());

		SetDirtyThis();
	}
	public void RecordNewTextOf(string npcName, string writtenKey)
	{
		var lastPage = chapterRecordPerNpc[npcName].pages[^1];
		lastPage.keysPerPage.Add(writtenKey);

		SetDirtyThis();
	}
	#endregion


	#region Latest Key Record Per Npc

	[SerializeField] UDictionary<string, string> latestDialogueKeyPerNpc;
	public UDictionary<string, string> LatestDialogueKeyPerNpc { get => latestDialogueKeyPerNpc; set => latestDialogueKeyPerNpc = value; }


	// latestKey -> 대화 중 갱신. 즉, 최근 키를 불러오는 최초 지점은 preprocess단계. Get에서 초기화해주면 됨
	public string GetLatestKeyOf(string npcName)
	{
		if (latestDialogueKeyPerNpc == null) latestDialogueKeyPerNpc = new();
		if (!latestDialogueKeyPerNpc.Keys.Contains(npcName)) latestDialogueKeyPerNpc[npcName] = "";
		var latestDialogueKey = latestDialogueKeyPerNpc[npcName];
		
		return latestDialogueKey;
	}
	public void UpdateLatestKeyOf(string npcName, string usedDialogueKey)
	{
		latestDialogueKeyPerNpc[npcName] = usedDialogueKey;
		SetDirtyThis();
	}

	#endregion



	#region ONLY BEFORE BUILD
	public void ResetDialogue()
	{
		ChapterRecordPerNpc = new();
		latestDialogueKeyPerNpc = new();
		askedQuestionSubjectPerNpc = new();

		SetDirtyThis();
	}


	#endregion



	[System.Serializable]
	public class Chapter
	{
		public Chapter() {  pages = new();  }
		public List<Page> pages;
	}

	[System.Serializable]
	public class Page
	{
		public Page() {  keysPerPage = new();  }
		public List<string> keysPerPage;
	}

	[System.Serializable]
	public class QuestionList
	{
		public QuestionList() { questionList = new(); }
		public List<string> questionList;
	}

}


