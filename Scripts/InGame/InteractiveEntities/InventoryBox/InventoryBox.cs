using UnityEngine;


public class InventoryBox : MonoBehaviour
{
	[SerializeField] private UDictionary<Evidence, GameObject> objectPerEvidence;
	[SerializeField] private PlaceInfo placeInfo;

	public UDictionary<Evidence, GameObject> ObjectPerEvidence { get => objectPerEvidence; }


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


	private void OnItemObtained(Evidence obtainedEvidence)
	{
		ActivateObtainedItem(obtainedEvidence);
	}


	private void ActivateObtainedItems()
	{
		foreach(var obtainedEvidence in placeInfo.GetObtainedEvidences())
		{
			ActivateObtainedItem(obtainedEvidence);
		}

	}

	private void ActivateObtainedItem(Evidence obtainedItem)
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


	private void DeActivateAllItems()
	{
		foreach (var item in objectPerEvidence)
		{
			var itemObj = item.Value;
			if (itemObj.activeSelf) itemObj.SetActive(false);
		}
	}


}
