using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResetter : MonoBehaviour
{

	[SerializeField] PhotoEvidenceManager photoEvidenceManager;
	[SerializeField] PhotoEvidenceBoard photoEvidenceBoard;


	public void ResetPhotoEvidences()
	{
		//photoEvidenceBoard.ResetTargetPhase();
		photoEvidenceManager.ResetPhotoEvidenceDic();

		Debug.Log("Photo Evidences are Resetted!");
	}



	[SerializeField, Space(15)] PlayerInfo playerInfo;
	public void ResetInventory()
	{
		playerInfo.CurrentPlace.ResetEvidences();
	}



	[SerializeField, Space(15)] TutorialInfo tutorialInfo;
	public void ResetTutorial()
	{
		tutorialInfo.SetTutorialStatus(false);

		Debug.Log("Tutorial is Resetted!");
	}



	[SerializeField, Space(15)] NotebookInfo notebookInfo;
	public void ResetNpcDialogueWithPECleared()
	{
		CleanNpcDialogueHighlight();
		notebookInfo.ResetDialogue();

		Debug.Log("Dialogue is Resetted!");
	}


	[SerializeField, Space(15)] DialogueInfo dialogueInfo;
	public void CleanNpcDialogueHighlight()
	{
		dialogueInfo.CleanNpcDialogue();

		Debug.Log("Npc Dialogues are Cleaned!");
	}

}
