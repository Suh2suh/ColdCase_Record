using UnityEngine;
using UnityEditor;

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

		switch(interactiveEntityInfo.ObjectType)
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
