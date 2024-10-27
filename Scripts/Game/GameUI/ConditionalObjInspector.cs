using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConditionalObjInspector : MouseHoverChecker
{

	#region Variables

	[SerializeField, HideInInspector] DialogueInfo dialogueInfo;

	[SerializeField] InspectorPlacement inspectorPlacement;
	[SerializeField] InspectorCondition inspectorCondition;

	// Title, Summary 선택 가능하게 할 시, Instantiate 해야 함
	[Space(15), SerializeField] GameObject inspectorPanel;

	[Space(15), SerializeField] string sheetName = "ItemExplanation";
	[SerializeField]					   string itemKey;

	static int closenessFromCenter = 2;


	#endregion


	#region Initializaition

	enum InspectorPlacement { OnRight, OnLeft, OnBottom, OnObject }
	enum InspectorCondition { OnHover, OnObservation }

	private void Awake()
	{
		ObjInteractor.OnObservation += OnObservation;
	}

	protected override void Start()
	{
		if (inspectorCondition == InspectorCondition.OnHover)
			if (this.gameObject.layer != 8)   this.gameObject.layer = 8;

		DeActivateInspector();
	}

	private void OnDestroy()
	{
		ObjInteractor.OnObservation -= OnObservation;
	}


	#endregion



	public void SetItemKey(string newItemKey) { itemKey = newItemKey; }


	#region Inspector On Hover

	protected override void OnMouseHover()
	{
		// IF THIS IS ITEM IN INVENTORY -> ONLY ON INVENTORY MODE
		if (mouseHovableStatus != PlayerStatusManager.GetCurrentInterStatus()) return;

		if(inspectorCondition == InspectorCondition.OnHover)
			ActivateInspector();
	}
	protected override void OnMouseLeave()
	{
		// IF THIS IS ITEM IN INVENTORY -> ONLY ON INVENTORY MODE
		//if (mouseHovableStatus != PlayerStatusManager.GetCurrentInterStatus()) return;

		if (inspectorCondition == InspectorCondition.OnHover)
			DeActivateInspector();
	}


	#endregion


	#region Inspector On Observation

	void OnObservation(Transform ObservingObj, bool isOn)
	{
		if (inspectorCondition != InspectorCondition.OnObservation) return;

		if(ObservingObj == this.transform)
		{
			if (isOn) ActivateInspector();
			else       DeActivateInspector();
		}
	}


	#endregion



	public void ActivateInspector()
	{
		PreProcessInspector();

		ActivationController.ActivateObj(inspectorPanel, true);
	}

	void DeActivateInspector()
	{
		ActivationController.ActivateObj(inspectorPanel, false);
	}

	#region Inspector Activation Processing

	void PreProcessInspector()
	{
		SetInsepctorText();
		SetInspectorPos();


		// TODO: 코드 정리
		if(mouseHovableStatus == InteractionStatus.Inventory)
		{
			if (transform.TryGetComponent<PlayerCheckStatus>(out var playerCheckStatus))
				playerCheckStatus.SetStatusChecked();
		}
	}



	void SetInsepctorText()
	{
		string itemSummaryText;

		if (dialogueInfo.CommonDialogueDictionary[sheetName].Keys.Contains(itemKey))
			itemSummaryText = dialogueInfo.CommonDialogueDictionary[sheetName][itemKey][dialogueInfo.language];
		else
			itemSummaryText = "[NOT FOUND] : Item Key is not found in item dialogue";

		inspectorPanel.GetComponentInChildren<TextMeshProUGUI>().text = itemSummaryText;

	}

	void SetInspectorPos()
	{
		Vector2 inspectorPos = new();

		switch(inspectorPlacement)
		{
			case InspectorPlacement.OnLeft:
				inspectorPos = ScreenPositionGetter.GetScreenPosition(HorizontalAlignment.Left, VerticalAlignment.Center, closenessFromCenter);

				break;
			case InspectorPlacement.OnRight:
				inspectorPos =ScreenPositionGetter.GetScreenPosition(HorizontalAlignment.Right, VerticalAlignment.Center, closenessFromCenter);

				break;
			case InspectorPlacement.OnBottom:
				inspectorPos = ScreenPositionGetter.GetScreenPosition(HorizontalAlignment.Center, VerticalAlignment.Bottom, closenessFromCenter);

				break;
			case InspectorPlacement.OnObject:
				inspectorPos = Camera.main.WorldToScreenPoint(transform.position);

				break;
		}

		inspectorPanel.GetComponent<RectTransform>().position = inspectorPos;

	}
	


	#endregion



}
