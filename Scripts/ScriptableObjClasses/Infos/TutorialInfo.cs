using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TutorialInfo", menuName = "ScriptableObjects/Informations/TutorialInfo", order = 2)]
public class TutorialInfo : ScriptableObject
{
	// 해당 순서대로 툴 튜토리얼 진행
	// List<string> tutorialSequenceForTools = new() { "Polaroid", "Photo", "ClipBoard", "Notebook", "InventoryBox" };

	//[SerializeField] List<DetectiveTool> tutorialedTools; // 해당 툴을 내려놓고 나서, tutorial tool에 추가
	public Transform currentTutorialTool = null;

	[SerializeField] bool isTutorialEnd;
	public bool IsTutorialEnd { get => isTutorialEnd; set => isTutorialEnd = value; }

	public static System.Action<Transform> OnDetectiveToolTutorialed;

	public void SetTutorialStatus(bool isEnd)
	{
		isTutorialEnd = isEnd;

#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}

	/*
	void SetToolAsTutorialed(DetectiveTool detectiveTool)
	{

		if (tutorialedTools == null) tutorialedTools = new();

		// var isToolNameValid = tutorialSequenceForTools.Contains(detectiveTool.name);
		// if (isToolNameValid && !tutorialedTools.Contains(detectiveTool))
		if (!tutorialedTools.Contains(detectiveTool))
		{
			tutorialedTools.Add(detectiveTool);
			detectiveTool.isTutorialed = true;

			// next tool -> tutorialSequenceForTools[tutorialedTools.Count]
		}
	}


	public void ResetTutorialedTools()
	{
		foreach (var tool in tutorialedTools)
			tool.isTutorialed = false;

		tutorialedTools.Clear();
		isTutorialEnd = false;
	}
	*/


}
