using UnityEditor;
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




[CustomEditor(typeof(GameResetter))]
public class GameResetterEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var gameResetter = target as GameResetter;

		if (GUILayout.Button("[RESET] Inventory"))
		{
			gameResetter.ResetInventory();
		}
		if (GUILayout.Button("[RESET] Photo Evidence"))
		{
			gameResetter.ResetPhotoEvidences();
		}
		if (GUILayout.Button("[RESET] Tutorial"))
		{
			gameResetter.ResetTutorial();
		}
		if (GUILayout.Button("[RESET] Dialogue"))
		{
			gameResetter.ResetNpcDialogueWithPECleared();
		}
		if (GUILayout.Button("[CLEAN] Dialogue Highlight"))
		{
			gameResetter.CleanNpcDialogueHighlight();
		}
	}


}