using UnityEditor;
using UnityEngine;


public class InteractiveEntityInfo : MonoBehaviour
{
	[Space(15)]
	[Tooltip("If Interactive Furniture, must attatch InteractiveFurnitureControllers")]
	[SerializeField] ObjectType objectType = ObjectType.None;

	[Space(5)]
	[Tooltip("If it is False, you cannot Interact with Mouse Or CrossHair")]
	[SerializeField] bool isInteractive = true;

	public ObjectType ObjectType { get => objectType; }
	public bool IsInteractive
	{
		get => isInteractive;
		set => isInteractive = value;
	}

	// Dynamic: Show | Hide By Object Type
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
	#region Properties - Dynamic
	public ObservableObject ObservableObjectInfo { get => observableObjectInfo; }
	public ObtainableObject ObtainableObjectInfo { get => obtainableObjectInfo; }
	public ObservablePlace ObservablePlaceInfo { get => observablePlaceInfo; }
	public Npc NpcInfo { get => npcInfo; }

	#endregion


	#region Unity Methods

	private void Start()
	{
		switch (this.objectType)
		{
			case ObjectType.ObservableObj:
				observableObjectInfo.initialPos = this.transform.position;
				observableObjectInfo.initialRot = this.transform.rotation;

				break;
			case ObjectType.ObtainableObj:
				obtainableObjectInfo.DeActivateIfIsObtained(this.gameObject);
				isInteractive = !IsUnObtainedPhotoEvidence(this.transform);

				break;
			case ObjectType.NPC:
				if (npcInfo.NpcName != "" && npcInfo.NpcPlace != null)
					npcInfo.NpcPlace.AddNpcInPlace(npcInfo.NpcName);

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




[CustomEditor(typeof(InteractiveEntityInfo))]
public class InteractiveEntityInfoEditor : Editor
{

	public override void OnInspectorGUI()
	{
		var interactiveEntityInfo = (InteractiveEntityInfo)target;

		var objectType = serializedObject.FindProperty("objectType");
		EditorGUILayout.PropertyField(objectType);

		var isInteractive = serializedObject.FindProperty("isInteractive");
		EditorGUILayout.PropertyField(isInteractive);

		switch (interactiveEntityInfo.ObjectType)
		{
			case ObjectType.ObservableObj:
				var observableObjectInfo = serializedObject.FindProperty("observableObjectInfo");
				EditorGUILayout.PropertyField(observableObjectInfo, true);

				break;
			case ObjectType.ObtainableObj:
				var obtainableObjectInfo = serializedObject.FindProperty("obtainableObjectInfo");
				EditorGUILayout.PropertyField(obtainableObjectInfo, true);

				break;
			case ObjectType.ObservablePlace:
			case ObjectType.DetectiveDesk:
			case ObjectType.Inventory:
			case ObjectType.WalkieTalkie:
				var observablePlaceInfo = serializedObject.FindProperty("observablePlaceInfo");
				EditorGUILayout.PropertyField(observablePlaceInfo, true);

				break;
			case ObjectType.NPC:
				var npcInfo = serializedObject.FindProperty("npcInfo");
				EditorGUILayout.PropertyField(npcInfo, true);

				break;
			default:
				break;
		}

		serializedObject.ApplyModifiedProperties();
	}


}

