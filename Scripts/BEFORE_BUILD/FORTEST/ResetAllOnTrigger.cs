using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetAllOnTrigger : MonoBehaviour
{
	bool isResettedInThisStage = false;
	
	[SerializeField] GameResetter gameResetter;
	[SerializeField] PlaceInfo placeInfo;

	private void OnTriggerEnter(Collider other)
	{
		if( ! isResettedInThisStage)
		{
			placeInfo.isPlaceCleared = false;
			placeInfo.SetPhase('Z');
			//playerInfo.SetPhase('D');

			gameResetter.ResetInventory();
			gameResetter.ResetNpcDialogueWithPECleared();
			gameResetter.ResetPhotoEvidences();
			gameResetter.ResetTutorial();

			isResettedInThisStage = true;
		}
	}
}
