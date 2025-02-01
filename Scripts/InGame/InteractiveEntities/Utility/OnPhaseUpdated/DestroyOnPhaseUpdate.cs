using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnPhaseUpdate : InvokeOnPhaseUpdate
{
	[SerializeField] PlaceAndPhase destroyCondition;

	private void Start()
	{
		// if currentPlace == destroyCondition.placeInfo &&
		if (PhaseChecker.GetCurrentPhase() >= destroyCondition.phase && PhaseChecker.GetCurrentPhase() != 'Z')
			Destroy(this.gameObject);
	}

	protected override void OnPhaseUpdated(PlaceInfo phaseUpdatedPlace)
	{
		if(phaseUpdatedPlace == destroyCondition.placeInfo &&
			PhaseChecker.GetCurrentPhase() == destroyCondition.phase)
		{
			Destroy(this.gameObject);
		}
	}

	[System.Serializable]
	struct PlaceAndPhase
	{
		public PlaceInfo placeInfo;
		public char phase;
	}
}
