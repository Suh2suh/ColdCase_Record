using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public class ObjectMouseObservationHandler : ObjectObservationHandlerBase
{
	// TOOD: [250127] TutorialInfo tutorialInfo -> 튜토리얼 정보 변수 필요없도록 고치기
	public TutorialInfo tutorialInfo;
	public ObjectMouseObservationHandler() { }
	public ObjectMouseObservationHandler(float observeDuration, float rotSpeed, CancellationToken cancellationToken, TutorialInfo tutorialInfo)
	: base(observeDuration, rotSpeed, cancellationToken)
	{ 
		this.tutorialInfo = tutorialInfo;
	}


	public override bool canStartInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.Investigating &&
			   ObjectSorter.MouseHoveringObj.ObjType == ObjectType.ObservableObj;
	}
	public override bool canEscapeInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingObject &&
			   PlayerStatusManager.PrevInterStatus == InteractionStatus.Investigating;
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

		observableObject.Set(ObjectSorter.MouseHoveringObj);

		var playerCheckStatusControllers = observableObject.ObjTransform.GetComponentsInChildren<PlayerCheckStatus>();
		if (observableObject.ObjTransform.GetComponentsInChildren<PlayerCheckStatus>() != null)
		{
			foreach (var playerCheckStatusController in playerCheckStatusControllers)
				playerCheckStatusController.SetStatusChecked();
		}
		DisableEmissionOn(observableObject.ObjTransform);

		return UniTask.FromResult(true);
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

		extraPostProcess?.Invoke();
		// TODO: [250127] 해당 부분 추후 다른 곳으로 옮기도록 방안 강구
		if (!tutorialInfo.IsTutorialEnd && TutorialInfo.OnDetectiveToolTutorialed != null)
		{
			TutorialInfo.OnDetectiveToolTutorialed(observableObject.ObjTransform);
			if( observableObject.ObjTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
				mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
		}

		observableObject.Set(null);
	}


	#region Utility
	// 추후 
	private void DisableEmissionOn(Transform objTransform)
	{
		if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
			mouseHoverEmitter.ForceStopEmitAndBlink();
	}

	#endregion


}