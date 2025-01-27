using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;


public abstract class ObjectObservationHandlerBase : InteractionHandlerBase
{
	public IObjectInfo observableObject;

	public float observeDuration;
	public float rotSpeed;
	public CancellationToken cancellationToken;

	public ObjectObservationHandlerBase() { }
	public ObjectObservationHandlerBase(float observeDuration, float rotSpeed, CancellationToken cancellationToken)
	{
		Initialize(observeDuration, rotSpeed, cancellationToken);
	}
	public void Initialize(float observeDuration, float rotSpeed, CancellationToken cancellationToken)
	{
		this.observeDuration = observeDuration;
		this.rotSpeed = rotSpeed;
		this.cancellationToken = cancellationToken;
	}


	// < should Implement in childs > : differ by mouse / crosshair
	// shouldStartInteraction
	// PreProcess
	// shouldEscapeInteraction
	// PostProcess


	protected override void ProcessStart()
	{
		MoveObservingObj().Forget();

		if (observableObject.ObjInteractInfo.ObservableObjectInfo.IsRotatable)
			RotateObjInObservation().Forget();
	}

	private async UniTaskVoid MoveObservingObj()
	{
		//isLerpEventOn = true;
		var observingObjInfo = observableObject.ObjInteractInfo.ObservableObjectInfo;

		float zoomDistance = observingObjInfo.ZoomDistance;
		Vector3 observePos = Camera.main.ScreenToWorldPoint(ScreenPositionGetter.GetScreenPosition(observingObjInfo.ScreenPosition, zoomDistance));

		bool lerpSucceed = false;
		if (observingObjInfo.IsFaceCamera)
		{
			var observeRot = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
			lerpSucceed = await ObjectLerper.LerpObjTransformAsync(observableObject.ObjTransform, observePos, observeRot, observeDuration, Space.World,
																   cancellationToken);
		}
		else
		{
			lerpSucceed = await ObjectLerper.LerpObjTransformAsync(observableObject.ObjTransform, observePos, observeDuration, Space.World,
															       cancellationToken);
		}

		if (lerpSucceed)
			ObjInteractor.OnObservation(observableObject.ObjTransform, true);
		// isLerpEventOn = false;
	}

	private async UniTaskVoid RotateObjInObservation()
	{
		while (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject)
		{
			ObjInteractor.RotateObjOnDrag(observableObject.ObjTransform, rotSpeed);

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
		}
	}


	protected override async UniTask<bool> ProcessEnd()
	{
		ObjInteractor.OnObservation(observableObject.ObjTransform, false);

		//isLerpEventOn = true;
		bool succeed = await ObjectLerper.LerpObjTransformAsync(observableObject.ObjTransform,
																observableObject.ObjInteractInfo.ObservableObjectInfo.objLocalPos,
																observableObject.ObjInteractInfo.ObservableObjectInfo.objRot,
																observeDuration, Space.Self, cancellationToken);
		//isLerpEventOn = false;

		return succeed;
	}


}