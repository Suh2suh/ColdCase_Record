using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlaceInfo", menuName = "ScriptableObjects/Informations/PlaceInfo", order = 1)]
public class PlaceInfo : ScriptableObject
{
	// Tutorial, House1, House2등, 각 장소에 따른 정보 저장
	// Ex. 해당 장소의 Npc 목록이라거나, 혹은, 해당 장소에서 얻어야 하는 모든 증거들...
	// 현재는 Scriptable Object로 하되, 추후 string으로 바꿔도 됨.
	// 하지만, Scriptable Object(Evidence)가 Photo Evidence Check, Inventory에 아울러 쓰이기 때문에 충분하다고 생각함.
	/*
	
	// Obtained: inventory Box에서 확인
	// Checked: 
	 
	 Place enum을 해당 클래스로 대체 -> 확장성 부여,
	Npc별로 Place를 다르게 했기 때문에, NotebookController에서 Notebook 들 때,
	해당 Place 같이 전달.
	NpcDialgoue에서, 현재 들고있는 Notebook의 PlaceInfo받아서, allEvidencesInPlace를 C페이즈에 질문하면 해결.

	evidence가 다 클리어되었는지는, phaseChecker에서 확인하기로 하고,
	static으로 해서 Place Info 전달, 그러면 거기서 allEvidencesInPlace가 isObtained되었는지 & 확인되엇는지 Check
	 -> 후에, Phase Update 가능하다고 알림.
	 
	 
	 
	 */
	new void SetDirty()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}


	// Evidence, Notebook 은 따로 저장할 필요가 있어보임.

	[SerializeField] char phase;
	public bool isPlaceCleared;



	[Space(15)]
	[SerializeField] List<Evidence> allEvidencesInPlace;



	[Space(15)]
	[SerializeField] NotebookInfo notebookInPlace;
	List<string> allNpcsInPlace = new(); // npc class에서, 매 게임 시작마다 자동으로 추가해줌.



	public static System.Action<PlaceInfo, char> OnPhaseUpdated;
	public static System.Action<PlaceInfo> OnPlaceCleared;



	#region Getters / Setters

	[HideInInspector] public NotebookInfo NotebookInPlace { get => notebookInPlace; set => notebookInPlace = value; }



	[HideInInspector] public List<Evidence> AllEvidencesInPlace { get => allEvidencesInPlace; set => allEvidencesInPlace = value; }	
	[HideInInspector] public List<string> AllNpcsInPlace { get => allNpcsInPlace; }
	public void AddNpcInPlace(string npcName)
	{ if (!allNpcsInPlace.Contains(npcName)) allNpcsInPlace.Add(npcName); }



	[HideInInspector] public char Phase { get => phase; set => phase = value; }
	public void SetPhase(char newPhase)
	{
		phase = newPhase;
		if (OnPhaseUpdated != null) OnPhaseUpdated(this, newPhase);

		SetDirty();
	}


	#endregion



	public List<Evidence> GetObtainedEvidences()
	{
		var obtainedEvidences = new List<Evidence>();

		foreach(var evidence in allEvidencesInPlace)
		{
			if (evidence.IsObtained)
				obtainedEvidences.Add(evidence);
		}

		return obtainedEvidences;
	}


	public void ResetEvidences()
	{
		foreach (var item in GetObtainedEvidences())
		{
			item.SetIsObtained(false);
			item.SetIsChecked(false);
		}

		SetDirty();
	}



}