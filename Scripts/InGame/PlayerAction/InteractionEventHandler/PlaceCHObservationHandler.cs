using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public class PlaceCHObservationHandler : InteractionHandlerBase
{

	private IObjectInfo observablePlace;

	// TODO: [250127] 마찬가지, 플레이어 카메라 LERPER 싱글톤 하든지 뺄 방법 강구
	private CameraLerper playerCameraLerper;
	private CancellationToken cancellationToken;

	public void Initialize(CameraLerper playerCameraLerper, CancellationToken cancellationToken)
	{
		this.playerCameraLerper = playerCameraLerper; 
		this.cancellationToken = cancellationToken;
	}


	public override bool canStartInteraction
	{
		get => (PlayerStatusManager.CurrentInterStatus == InteractionStatus.None) &&
			   (ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservablePlace || ObjectSorter.CHPointingObj.ObjType == ObjectType.DetectiveDesk);
	}
	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingPlace || PlayerStatusManager.CurrentInterStatus == InteractionStatus.Investigating;
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
	{
		var interactionStatus = (ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservablePlace ?
								 InteractionStatus.ObservingPlace : InteractionStatus.Investigating);
		PlayerStatusManager.SetInterStatus(interactionStatus);

		observablePlace = ObjectSorter.CHPointingObj;

		return UniTask.FromResult(true);
	}


	protected async override UniTask ProcessStart()
	{
		var observablePlaceInfo = observablePlace.ObjInteractInfo.ObservablePlaceInfo;
		float observeDuration = observablePlaceInfo.PlaceObserveDuration;
		Transform observePos = observablePlaceInfo.ObservingPos;

		await playerCameraLerper.MoveToNewTransform(observePos.position, observePos.rotation,
													observeDuration, cancellationToken);
	}


	protected override async UniTask<bool> ProcessEnd()
	{
		var observablePlaceInfo = observablePlace.ObjInteractInfo.ObservablePlaceInfo;
		float observeDuration = observablePlaceInfo.PlaceObserveDuration;

		//isLerpEventOn = true;
		bool succeed = await playerCameraLerper.BackToPrevTransform(observeDuration, cancellationToken);
		//isLerpEventOn = false;

		return succeed;
	}

	protected override void PostProcess(Action extraPostProcess)
	{
		observablePlace = null;
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);
	}


}