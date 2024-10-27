using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBox : MonoBehaviour
{
	[SerializeField] UDictionary<Evidence, GameObject> objectPerEvidence;
	public UDictionary<Evidence, GameObject> ObjectPerEvidence { get => objectPerEvidence; }

	[SerializeField] PlaceInfo placeInfo;


	#region Unity Methods

	private void Awake()
	{
		Evidence.OnEvidenceObtained += OnItemObtained;
	}

	private void Start()
	{
		DeActivateAllItems();
		ActivateObtainedItems();
	}

	private void OnDestroy()
	{
		Evidence.OnEvidenceObtained -= OnItemObtained;
	}


	#endregion



	void OnItemObtained(Evidence obtainedEvidence)
	{
		ActivateObtainedItem(obtainedEvidence);
	}




	void ActivateObtainedItems()
	{
		foreach(var obtainedEvidence in placeInfo.GetObtainedEvidences())
		{
			ActivateObtainedItem(obtainedEvidence);
		}

	}


	void DeActivateAllItems()
	{
		foreach (var item in objectPerEvidence)
		{
			var itemObj = item.Value;
			if (itemObj.activeSelf) itemObj.SetActive(false);
		}
	}



	void ActivateObtainedItem(Evidence obtainedItem)
	{
		if (objectPerEvidence.ContainsKey(obtainedItem))
		{
			var obtainedItemObj = objectPerEvidence[obtainedItem];
			obtainedItemObj.SetActive(true);
		}
		else
		{
			Debug.LogWarning("Obtained Item [" + obtainedItem.name + "] not exists in Inventory Box List! ");
		}
	}


}
