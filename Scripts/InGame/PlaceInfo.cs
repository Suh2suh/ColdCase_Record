using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlaceInfo", menuName = "ScriptableObjects/Informations/PlaceInfo", order = 1)]
public class PlaceInfo : ScriptableObject
{
	// Tutorial, House1, House2��, �� ��ҿ� ���� ���� ����
	// Ex. �ش� ����� Npc ����̶�ų�, Ȥ��, �ش� ��ҿ��� ���� �ϴ� ��� ���ŵ�...
	// ����� Scriptable Object�� �ϵ�, ���� string���� �ٲ㵵 ��.
	// ������, Scriptable Object(Evidence)�� Photo Evidence Check, Inventory�� �ƿ﷯ ���̱� ������ ����ϴٰ� ������.
	/*
	
	// Obtained: inventory Box���� Ȯ��
	// Checked: 
	 
	 Place enum�� �ش� Ŭ������ ��ü -> Ȯ�强 �ο�,
	Npc���� Place�� �ٸ��� �߱� ������, NotebookController���� Notebook �� ��,
	�ش� Place ���� ����.
	NpcDialgoue����, ���� ����ִ� Notebook�� PlaceInfo�޾Ƽ�, allEvidencesInPlace�� C����� �����ϸ� �ذ�.

	evidence�� �� Ŭ����Ǿ�������, phaseChecker���� Ȯ���ϱ�� �ϰ�,
	static���� �ؼ� Place Info ����, �׷��� �ű⼭ allEvidencesInPlace�� isObtained�Ǿ����� & Ȯ�εǾ����� Check
	 -> �Ŀ�, Phase Update �����ϴٰ� �˸�.
	 
	 
	 
	 */
	new void SetDirty()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}


	// Evidence, Notebook �� ���� ������ �ʿ䰡 �־��.

	[SerializeField] char phase;
	public bool isPlaceCleared;



	[Space(15)]
	[SerializeField] List<Evidence> allEvidencesInPlace;



	[Space(15)]
	[SerializeField] NotebookInfo notebookInPlace;
	List<string> allNpcsInPlace = new(); // npc class����, �� ���� ���۸��� �ڵ����� �߰�����.



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