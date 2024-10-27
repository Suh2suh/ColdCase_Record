using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			DestoryThisObject();
	}
	private void OnDestroy()
	{
		PlaceInfo.OnPlaceCleared -= OnPlaceCleared;
	}

	#endregion



	void OnPlaceCleared(PlaceInfo placeInfo)
	{
		if(placeCondition == placeInfo)
			DestoryThisObject();
	}

	void DestoryThisObject()
	{
		Destroy(this.gameObject);
	}
}
