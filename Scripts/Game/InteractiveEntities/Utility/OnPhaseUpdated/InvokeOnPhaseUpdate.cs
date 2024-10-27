using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InvokeOnPhaseUpdate : MonoBehaviour
{
	private void Awake()
	{
		PhaseController.OnPhaseUpdated += OnPhaseUpdated;
	}

	private void OnDestroy()
	{
		PhaseController.OnPhaseUpdated -= OnPhaseUpdated;
	}

	protected abstract void OnPhaseUpdated(PlaceInfo phaseUpdatedPlace);
}
