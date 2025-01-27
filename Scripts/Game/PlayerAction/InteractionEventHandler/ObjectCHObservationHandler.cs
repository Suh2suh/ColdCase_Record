using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;


public class ObjectCHObservationHandler : ObjectObservationHandlerBase
{
	public ObjectCHObservationHandler() { }
	public ObjectCHObservationHandler(float observeDuration, float rotSpeed, CancellationToken cancellationToken)
		: base(observeDuration, rotSpeed, cancellationToken)
	{ }


	public override bool canStartInteraction
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None &&
			   ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservableObj;
	}
	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject &&
			   (PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.None || PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.Obtaining);
	}


	protected override async UniTask<bool> PreProcess(Action extraPreProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

		observableObject = ObjectSorter.CHPointingObj;

		return true;
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);

		extraPostProcess?.Invoke();

		observableObject = null;
	}


}