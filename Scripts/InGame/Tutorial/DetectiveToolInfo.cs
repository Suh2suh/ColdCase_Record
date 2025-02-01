using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Tutorial Tool Info라고 바꾸기
public class DetectiveToolInfo : PlayerCheckStatus
{
	[SerializeField] DetectiveTool toolType;
	[SerializeField] GameObject alertIconObj;

	#region Getters / Setters
	public DetectiveTool ToolType { get => toolType; }

	#endregion

	#region Unity Methods

	private void Awake()
	{
		TutorialManager.OnTutorialToolUpdated += OnTutorialToolUpdated;
	}

	private void Start()
	{
		if(toolType.isTutorialed && toolType.name == "Polaroid")
			if (this.gameObject.activeSelf) this.gameObject.SetActive(false);

		ActivateAlertIcon(false);
	}

	private void OnDestroy()
	{
		TutorialManager.OnTutorialToolUpdated -= OnTutorialToolUpdated;
	}

	#endregion

	void OnTutorialToolUpdated(DetectiveTool newTutorialTool)
	{
		if(newTutorialTool == toolType)
		{
			ActivateAlertIcon(true);
		}
	}

	public override void OnCheckedByPlayer()
	{
		ActivateAlertIcon(false);
	}


	void ActivateAlertIcon(bool activeStatus)
	{
		if (alertIconObj && alertIconObj.activeSelf != activeStatus) alertIconObj.SetActive(activeStatus);

		//Debug.Log(this.name + " : " + activeStatus);
	}
}
