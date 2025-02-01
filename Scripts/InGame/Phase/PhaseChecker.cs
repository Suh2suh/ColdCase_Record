using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseChecker : MonoBehaviour
{
	[SerializeField] PlaceInfo placeInfo;
	static PlaceInfo PlaceInfo;

	private void Awake()
	{
		PlaceInfo = placeInfo;
	}



	public static char GetCurrentPhase()
	{
		return PlaceInfo.Phase;
	}

	public static char GetNextPhase()
	{
		if (PlaceInfo.Phase == '\0' || PlaceInfo.Phase == 'Z')
			return 'A';

		char newPhase = (char)(PlaceInfo.Phase + 1);

		return newPhase;
	}


}
