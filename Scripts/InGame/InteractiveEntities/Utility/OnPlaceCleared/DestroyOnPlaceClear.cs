using UnityEngine;


// POSSIBLE: [250201] Destroy/Disable On Clear
public class DestroyOnPlaceClear : MonoBehaviour
{
	[SerializeField] PlaceInfo placeCondition;


	#region Unity Methods

	private void Awake()
	{
		PlaceInfo.OnPlaceCleared += OnPlaceCleared;
	}
	private void Start()
	{
		if (placeCondition.isPlaceCleared)
			Destroy(this.gameObject);
	}
	private void OnDestroy()
	{
		PlaceInfo.OnPlaceCleared -= OnPlaceCleared;
	}


	#endregion


	void OnPlaceCleared(PlaceInfo placeInfo)
	{
		if(placeCondition == placeInfo)
			Destroy(this.gameObject);
	}


}
