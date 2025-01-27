using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public class PlaceMouseObservationHandler : InteractionHandlerBase
{
	private IObjectInfo observablePlace;

	// TODO: [250127] playerCameraLerper은 하나뿐이므로, singleton으로 하나 만들든지 할것
	//                마찬가지로 tutorialInfo 뺄 방법 강구
	public CameraLerper playerCameraLerper;
	public TutorialInfo tutorialInfo;
	public CancellationToken cancellationToken;

	public void Initialize(CameraLerper playerCameraLerper, TutorialInfo tutorialInfo, CancellationToken cancellationToken)
	{
		this.playerCameraLerper = playerCameraLerper;
		this.tutorialInfo = tutorialInfo;
		this.cancellationToken = cancellationToken;
	}


	public override bool canStartInteraction
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating &&
			   (ObjectSorter.MouseHoveringObj.ObjType == ObjectType.WalkieTalkie || ObjectSorter.MouseHoveringObj.ObjType == ObjectType.Inventory);
	}

	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie ||
			   PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Inventory;
	}


	protected override async UniTask<bool> PreProcess(Action extraPreProcess)
	{
		switch(ObjectSorter.MouseHoveringObj.ObjType)
		{
			case ObjectType.WalkieTalkie:
				PlayerStatusManager.SetInterStatus(InteractionStatus.TalkingWalkieTalkie);
			
				break;
			case ObjectType.Inventory:
				PlayerStatusManager.SetInterStatus(InteractionStatus.Inventory);

				break;
			default:
				return false;
		}
		observablePlace = ObjectSorter.MouseHoveringObj;

		DisableEmissionOn(observablePlace.ObjTransform);
		if (observablePlace.ObjTransform.TryGetComponent<PlayerCheckStatus>(out var playerCheckStatusController))
			playerCheckStatusController.SetStatusChecked();
		if (observablePlace.ObjTransform.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
			mouseHoverChecker.IsMouseHovering = false;

		return true;
	}


	protected override void ProcessStart()
	{
		ProcessStartWrapper().Forget();
	}
	private async UniTaskVoid ProcessStartWrapper()
	{
		var mouseObservePos = observablePlace.ObjInteractInfo.ObservablePlaceInfo.ObservingPos;
		float observeDuration = observablePlace.ObjInteractInfo.ObservablePlaceInfo.PlaceObserveDuration;

		//isLerpEventOn = true;
		bool succeed = await playerCameraLerper.MoveToNewTransform(mouseObservePos.position, mouseObservePos.rotation, observeDuration,
																   cancellationToken);
		//isLerpEventOn = false;

		if (succeed)
		{
			if (observablePlace.ObjType == ObjectType.WalkieTalkie && tutorialInfo.IsTutorialEnd)
				DialogueInfo.OnWalkieTalkieDialogueStart();
		}
		// TODO: [250127] 고민: else -> processEnd()?: 필요성
	}


	protected override async UniTask<bool> ProcessEnd()
	{
		if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie && tutorialInfo.IsTutorialEnd)
			DialogueInfo.OnWalkieTalkieDialogueEnd();

		//isLerpEventOn = true;
		float observeDuration = observablePlace.ObjInteractInfo.ObservablePlaceInfo.PlaceObserveDuration;
		bool succeed = await playerCameraLerper.BackToPrevTransform(observeDuration, cancellationToken);
		//isLerpEventOn = false;

		return succeed;
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

		//TODO: [250124] BUGFIX
		if (observablePlace.ObjTransform.TryGetComponent<DetectiveToolInfo>(out var detectiveTool))
			if (!tutorialInfo.IsTutorialEnd)
				TutorialInfo.OnDetectiveToolTutorialed(detectiveTool.transform);
	}




	// TODO: [250127] 중복이니 다른 곳으로 무조건 옮길것
	#region WillBeDeprecated
	private void DisableEmissionOn(Transform objTransform)
	{
		if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
			mouseHoverEmitter.ForceStopEmitAndBlink();

		if (!tutorialInfo.IsTutorialEnd)
			mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
	}

	#endregion


}