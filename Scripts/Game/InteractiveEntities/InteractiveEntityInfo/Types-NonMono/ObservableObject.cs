using UnityEngine;


[System.Serializable]
public class ObservableObject
{
	[SerializeField] private bool isRotatable = true;
	[SerializeField] private bool isFaceCamera = false;


	[SerializeField, Range(0.15f, 1)] private float zoomDistance = 0.45f;
	[SerializeField] private ScreenPosition screenPosition;


	[HideInInspector] public Vector3 objLocalPos;
	[HideInInspector] public Quaternion objRot;

	#region Getters

	[HideInInspector] public bool IsRotatable { get => isRotatable; }
	[HideInInspector] public bool IsFaceCamera { get => isFaceCamera; }
	[HideInInspector] public float ZoomDistance { get => zoomDistance; }
	[HideInInspector] public ScreenPosition ScreenPosition { get => screenPosition; }
	


	#endregion
}
