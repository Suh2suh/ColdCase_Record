using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionEventController : MonoBehaviour
{
	// TODO: 나중에 상속 구조로 바꿀 것

	[SerializeField] DialogueInfo dialogueInfo;

	[Tooltip("Parent Of decision UIs")]
	[SerializeField] Transform playerCanvas;

	[SerializeField, Space(15)] GameObject decisionPanelPrefab;
	[SerializeField] GameObject decisionButtonPrefab;

	public System.Action<string>OnDecisionButtonClicked;

	Dictionary<string, GameObject> decisionButtonsPerKey = new();
	public List<GameObject> decisionButtons { get => decisionButtonsPerKey.Values.ToList(); }


	GameObject DecisionPanel;


	/*
	public void InvokeDecisionEventFrom(string firstDecisionKey, string targetNpcName, UDictionary<string, CommunicationData> targetNpcData)
	{
		// 무전기에서 할 때에는... NpcDialogueKeyGetter말고 뭐 CommonDialogueKeyGetter을 하든가 뭘 하든가 할 것
		decisionKeys = NpcDialogueKeyGetter.GetDecisionKeysFrom(firstDecisionKey, targetNpcData);
		decisionPanel = Instantiate(decisionPanelPrefab, playerCanvas, false);

		foreach (var decisionKey in decisionKeys)
		{
			var decisionButtonObj = Instantiate(decisionButtonPrefab, decisionPanel.transform, false);
			decisionButtonObj.name = decisionKey;

			// Highlighting되지 않은 Text 불러와야하기 때문에, 예외적으로 Clean Data에서 Load 필요
			decisionButtonObj.GetComponentInChildren<TextMeshProUGUI>().text
				= dialogueInfo.CleanNpcDialogueDictionary[targetNpcName][decisionKey].dialogueByLanguage[dialogueInfo.language];

			decisionButtonObj.GetComponent<Button>().onClick.AddListener(() => DecisionButtonOnClick(decisionButtonObj.name));
		}
	}*/

	public void InvokeDecisionEventWith(string sheetName, List<string> decisionKeys)
	{
		DecisionPanel = Instantiate(decisionPanelPrefab, playerCanvas, false);
		DecisionPanel.transform.SetAsFirstSibling();   // 패널에서 가려지게 하기 위해

		var decisionButtonParent = DecisionPanel.GetComponentInChildren<VerticalLayoutGroup>().transform;

		foreach (var decisionKey in decisionKeys)
		{
			var decisionButtonObj = Instantiate(decisionButtonPrefab, decisionButtonParent, false);
			decisionButtonObj.name = decisionKey;
			decisionButtonObj.GetComponentInChildren<TextMeshProUGUI>().text
				=  dialogueInfo.CommonDialogueDictionary[sheetName][decisionKey][dialogueInfo.language];

			decisionButtonObj.GetComponent<Button>().onClick.AddListener(() => DecisionButtonOnClick(decisionButtonObj.name));

			decisionButtonsPerKey[decisionKey] = decisionButtonObj;
		}

	}



	// npc D
	/// <summary>  DecisionKeys 안에 든 element 하나 당 decision button을 만든다  </summary>
	/// TODO: 현재는 DecisionEvent에서만 불러오는데, 워키토키에서는 뭐 다른 거 쓰든지 변수 추가해서 유동적으로 할 것
	public void CreateDecisionButtonsWith(List<string> decisionKeys, bool isTransparent)
	{
		// 무전기에서 할 때에는... NpcDialogueKeyGetter말고 뭐 CommonDialogueKeyGetter을 하든가 뭘 하든가 할 것
		//decisionKeys = NpcDialogueKeyGetter.GetDecisionKeysFrom(firstDecisionKey, targetNpcData);
		DecisionPanel = Instantiate(decisionPanelPrefab, playerCanvas, false);
		DecisionPanel.transform.SetAsFirstSibling();

		var decisionButtonParent = DecisionPanel.GetComponentInChildren<VerticalLayoutGroup>().transform;
		//Debug.Log(decisionButtonParent);

		if (isTransparent)   ActivateDecisionPanel(false);

		foreach (var decisionKey in decisionKeys)
		{
			var decisionButtonObj = Instantiate(decisionButtonPrefab, decisionButtonParent, false);
			decisionButtonObj.name = decisionKey;

			string buttonInnerText = "";
			string[] splits = decisionKey.Split('_');

			if (splits[0] == "A")
				buttonInnerText = dialogueInfo.CommonDialogueDictionary["InvestigationEvent"][decisionKey][dialogueInfo.language];
			else if (splits[0] == "C")
				buttonInnerText = dialogueInfo.CommonDialogueDictionary["ItemName"][splits[1]][dialogueInfo.language];
				//buttonInnerText = dialogueInfo.CommonDialogueDictionary["Item"][decisionKey][dialogueInfo.language];


			decisionButtonObj.GetComponentInChildren<TextMeshProUGUI>().text = buttonInnerText;
			decisionButtonObj.GetComponent<Button>().onClick.AddListener(() => DecisionButtonOnClick(decisionButtonObj.name));

			decisionButtonsPerKey[decisionKey] = decisionButtonObj;
		}
	}


	void DecisionButtonOnClick(string selectQuestionKey)
	{
		//DestroyDecisionPanel();
		//decisionButtons = new();

		OnDecisionButtonClicked(selectQuestionKey);
	}



	#region Decision Button Control

	public void SetButtonInteractable(string buttonName, bool isInteractable)
	{
		decisionButtonsPerKey[buttonName].GetComponent<Button>().interactable = isInteractable;
	}

	public void ChangeButtonColor(string buttonName, Color newColor)
	{
		decisionButtonsPerKey[buttonName].GetComponent<Image>().color = newColor;
	}


	#endregion


	#region Activation Control

	public void ActivateDecisionPanel(bool activateStatus)
	{
		if (DecisionPanel.activeSelf != activateStatus)   DecisionPanel.SetActive(activateStatus);
	}

	public void DestroyDecisionPanel()
	{
		if (DecisionPanel) Destroy(DecisionPanel);
	}


	#endregion


}