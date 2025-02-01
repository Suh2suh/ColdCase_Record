using UnityEngine;


[System.Serializable]
public class ObservableObject
{
	#region Setting Variables
	[SerializeField] private bool isRotatable = true;
	[SerializeField] private bool isFaceCamera = false;

	[SerializeField, Range(0.15f, 1)] private float zoomDistance = 0.45f;
	[SerializeField] private ScreenPosition screenPosition;

	#endregion

	[HideInInspector] public Vector3 initialPos;
	[HideInInspector] public Quaternion initialRot;

	public bool IsRotatable { get => isRotatable; }
	public bool IsFaceCamera { get => isFaceCamera; }
	public float ZoomDistance { get => zoomDistance; }
	public ScreenPosition ScreenPosition { get => screenPosition; }

}
