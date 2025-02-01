using UnityEngine;


[System.Serializable]
public class ObservablePlace
{
	#region Setting Varaibles
	[SerializeField] private Transform observingPos;
	[SerializeField, Range(0.5f, 1.0f)] private float placeObserveDuration = 0.5f;

	#endregion

	public Transform ObservingPos { get => observingPos; }
	public float PlaceObserveDuration { get => placeObserveDuration; }

}