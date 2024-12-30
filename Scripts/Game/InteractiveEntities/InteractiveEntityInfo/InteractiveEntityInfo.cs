using UnityEngine;


public class InteractiveEntityInfo: MonoBehaviour
{
	[Space(15)]
	[Tooltip("If Interactive Furniture, must attatch InteractiveFurnitureControllers")]
	[SerializeField] ObjectType objectType = ObjectType.None;

	[Space(5)]
	[Tooltip("If it is False, you cannot Interact with Mouse Or CrossHair")]
	[SerializeField] bool isInteractive = true;

	// Show | Hide By Object Type
	#region Setting Variables - Dynamic
	[Space(15)]
	[SerializeField] ObservableObject observableObjectInfo;

	[Space(15)]
	[SerializeField] ObtainableObject obtainableObjectInfo;

	[Space(15)]
	[SerializeField] ObservablePlace observablePlaceInfo;

	[Space(15)]
	[SerializeField] Npc npcInfo;

	#endregion

	#region Properties
	public ObjectType ObjectType { get => objectType; }
	public bool IsInteractive 
	{ 
		get => isInteractive;  
		set
		{
			if (isInteractive != value)
				isInteractive = value;
		}
	}

	[HideInInspector] public ObservableObject ObservableObjectInfo { get => observableObjectInfo; }
	[HideInInspector] public ObtainableObject ObtainableObjectInfo { get => obtainableObjectInfo; }
	[HideInInspector] public ObservablePlace ObservablePlaceInfo { get => observablePlaceInfo; }
	[HideInInspector] public Npc NpcInfo { get => npcInfo; }

	#endregion


	#region Unity Methods

	private void Start()
	{
		switch (this.objectType)
		{
			case ObjectType.ObservableObj:
				observableObjectInfo.objLocalPos = this.transform.localPosition;
				observableObjectInfo.objRot = this.transform.rotation;

				break;
			case ObjectType.ObtainableObj:
				obtainableObjectInfo.DeActivateIfIsObtained(this.gameObject);
				isInteractive = !IsUnObtainedPhotoEvidence(this.transform);

				break;
			case ObjectType.NPC:
				if (npcInfo.NpcName != "" && npcInfo.NpcPlace != null) npcInfo.NpcPlace.AddNpcInPlace(npcInfo.NpcName);

				break;
		}
	}


	#endregion


	private static bool IsUnObtainedPhotoEvidence(Transform obtainableObject)
	{
		var photoEvidenceCandidate = obtainableObject.GetComponent<PhotoEvidenceInfo>();
		if (photoEvidenceCandidate != null && photoEvidenceCandidate.EvidenceType.IsObtained == false)
			return true;

		return false;
	}


}


public enum ObjectType
{ 
	None, 

	ObservableObj, 
	ObtainableObj, 

	ObservablePlace, DetectiveDesk, Inventory, WalkieTalkie, 

	NPC, 

	InteractiveFurniture 
}
