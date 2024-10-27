using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceCheckStatusController : PlayerCheckStatus
{
	[SerializeField] Evidence evidenceInfo;
	[SerializeField] GameObject alertIconObj;

	#region Unity Methods
	private void Awake()
	{
		Evidence.OnEvidenceObtained += OnEvidenceObtained;

		ActivateAlertIcon(false);
	}

	private void Start()
	{
		//Debug.Log(evidenceInfo.name + ": " + evidenceInfo.IsObtained + " " + evidenceInfo.IsChecked);

		if ( evidenceInfo.IsObtained && ! evidenceInfo.IsChecked) ActivateAlertIcon(true);
		else ActivateAlertIcon(false);
	}

	private void OnDestroy()
	{
		Evidence.OnEvidenceObtained -= OnEvidenceObtained;
	}

	#endregion


	void OnEvidenceObtained(Evidence evidenceInfo)
	{
		if (evidenceInfo == this.evidenceInfo)
			ActivateAlertIcon(true);
	}



	public override void SetStatusChecked()
	{
		if (isCheckedByPlayer == true || ! evidenceInfo.IsObtained) return;

		isCheckedByPlayer = true;
		OnCheckedByPlayer();
	}

	public override void OnCheckedByPlayer()
	{
		evidenceInfo.SetIsChecked(true);
		ActivateAlertIcon(false);
	}





	void ActivateAlertIcon(bool activeStatus)
	{
		if (alertIconObj.activeSelf != activeStatus) alertIconObj.SetActive(activeStatus);
		//Debug.Log(this.name + " : "  + activeStatus);
	}
}
