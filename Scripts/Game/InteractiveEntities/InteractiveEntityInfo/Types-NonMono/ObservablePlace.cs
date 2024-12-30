using UnityEngine;


[System.Serializable]
public class ObservablePlace
{
	[SerializeField] private Transform observingPos;
	[SerializeField, Range(0.5f, 1.0f)] private float placeObserveDuration = 0.5f;


	#region Getters
	[HideInInspector] public Transform ObservingPos { get => observingPos; }
	[HideInInspector] public float PlaceObserveDuration { get => placeObserveDuration; }
	#endregion
}