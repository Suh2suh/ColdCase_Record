using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public class PlaceMouseObservationHandler : InteractionHandlerBase
{
	private ObjectInfo observablePlace = new();

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
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.Investigating &&
			   (ObjectSorter.MouseHoveringObj.ObjType == ObjectType.WalkieTalkie || ObjectSorter.MouseHoveringObj.ObjType == ObjectType.Inventory);
	}

	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.TalkingWalkieTalkie ||
			   PlayerStatusManager.CurrentInterStatus == InteractionStatus.Inventory;
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
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
				return UniTask.FromResult(false);
		}
		observablePlace.Set(ObjectSorter.MouseHoveringObj);

		DisableEmissionOn(observablePlace.ObjTransform);
		if (observablePlace.ObjTransform.TryGetComponent<PlayerCheckStatus>(out var playerCheckStatusController))
			playerCheckStatusController.SetStatusChecked();
		if (observablePlace.ObjTransform.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
			mouseHoverChecker.IsMouseHovering = false;

		return UniTask.FromResult(true);
	}


	protected override async UniTask ProcessStart()
	{
		var mouseObservePos = observablePlace.ObjInteractInfo.ObservablePlaceInfo.ObservingPos;
		float observeDuration = observablePlace.ObjInteractInfo.ObservablePlaceInfo.PlaceObserveDuration;

		Debug.Log("Start1: " + observablePlace.ObjTransform.name);
		bool succeed = await playerCameraLerper.MoveToNewTransform(mouseObservePos.position, mouseObservePos.rotation, observeDuration,
																   cancellationToken);
		Debug.Log(succeed);
		Debug.Log("Start2: " + observablePlace.ObjTransform.name);

		if (succeed)
		{
			if (observablePlace.ObjType == ObjectType.WalkieTalkie && tutorialInfo.IsTutorialEnd)
				DialogueInfo.OnWalkieTalkieDialogueStart();
		}
	}


	protected override async UniTask<bool> ProcessEnd()
	{
		if (PlayerStatusManager.CurrentInterStatus == InteractionStatus.TalkingWalkieTalkie && tutorialInfo.IsTutorialEnd)
			DialogueInfo.OnWalkieTalkieDialogueEnd();

		float observeDuration = observablePlace.ObjInteractInfo.ObservablePlaceInfo.PlaceObserveDuration;
		bool succeed = await playerCameraLerper.BackToPrevTransform(observeDuration, cancellationToken);

		return succeed;
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		if (observablePlace.ObjTransform.TryGetComponent<DetectiveToolInfo>(out var detectiveTool))
		{
			if (!tutorialInfo.IsTutorialEnd)
				TutorialInfo.OnDetectiveToolTutorialed(detectiveTool.transform);
		}

		observablePlace.Set(null);
		PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);
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