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
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.None &&
			   ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservableObj;
	}
	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingObject &&
			   (PlayerStatusManager.PrevInterStatus == InteractionStatus.None || PlayerStatusManager.PrevInterStatus == InteractionStatus.Obtaining);
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

		observableObject = ObjectSorter.CHPointingObj;

		return UniTask.FromResult(true);
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);

		extraPostProcess?.Invoke();

		observableObject = null;
	}


}