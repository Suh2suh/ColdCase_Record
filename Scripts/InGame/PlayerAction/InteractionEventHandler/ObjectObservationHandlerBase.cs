using Cysharp.Threading.Tasks;
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


	protected override async UniTask ProcessStart()
	{
		// <if you want player can rotate obj during moving it>
		// if (observableObject.ObjInteractInfo.ObservableObjectInfo.IsRotatable)
		// RotateObjInObservation().Forget();

		await MoveObservingObj();

		if (observableObject.ObjInteractInfo.ObservableObjectInfo.IsRotatable)
			RotateObjInObservation().Forget();
	}

	private async UniTask MoveObservingObj()
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
		while (PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingObject)
		{
			observableObject.ObjTransform.RotateOnDrag(rotSpeed);

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
		}
	}


	protected override async UniTask<bool> ProcessEnd()
	{
		ObjInteractor.OnObservation(observableObject.ObjTransform, false);

		bool succeed = await ObjectLerper.LerpObjTransformAsync(observableObject.ObjTransform,
																observableObject.ObjInteractInfo.ObservableObjectInfo.initialPos,
																observableObject.ObjInteractInfo.ObservableObjectInfo.initialRot,
																observeDuration, Space.World, cancellationToken);

		return succeed;
	}


}