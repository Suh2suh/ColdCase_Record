using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;


public abstract class ObjectObtainmentHandlerBase : InteractionHandlerBase
{

	protected static IObjectInfo obtainableObject;
	protected static bool isObtainableObjDetected;

	private float obtainDuration;
	private MaterialEffectManager materialEffectController;
	private CancellationToken cancellationToken;


	#region Initializaiton

	public void Initialize(float obtainDuration, MaterialEffectManager materialEffectController,
						   CancellationToken cancellationToken)
	{
		this.obtainDuration = obtainDuration;
		this.materialEffectController = materialEffectController;
		this.cancellationToken = cancellationToken;

		DetectObtainableObjectAsync(cancellationToken).Forget();
	}
	private async UniTaskVoid DetectObtainableObjectAsync(CancellationToken cancellationToken)
	{
		while(true)
		{
			if (IsObtainableStatus)
			{
				obtainableObject = GetDetectedObtainableObj();
				isObtainableObjDetected = (obtainableObject != null);

				//TODO: 추후 ObjectInteractor에서 관리
				//if (ObtainableObj != null)
				//isObtainableObjDetected = CameraViewportObjectChecker.CheckObjSeenOnCamera(ObtainableObj.ObjTransform);
			}
			else
			{
				isObtainableObjDetected = false;
			}

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
		}
	}


	#endregion
	public abstract bool IsObtainableStatus { get; }

	/// <summary>
	/// Set obtainableObject here!
	/// </summary>
	protected abstract IObjectInfo GetDetectedObtainableObj();


	public override bool canStartInteraction
	{
		get => PhaseChecker.GetCurrentPhase() >= 'B' &&
			   isObtainableObjDetected &&
			   obtainableObject.ObjInteractInfo.IsInteractive;
	}
	public override bool canEscapeInteraction
	{
		get => false;   // Obtainment event is not escapable, since it's processed just for moment
	}


	protected override async UniTask<bool> PreProcess(Action extraPreProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.Obtaining);
		extraPreProcess?.Invoke();

		return true;
	}


	protected override void ProcessStart()
	{
		ProcessStartWrapper().Forget();
	}
	private async UniTaskVoid ProcessStartWrapper()
	{
		await StartObtainmentEffectAsync();

		ProcessEnd().Forget();
	}
	private async UniTask StartObtainmentEffectAsync()
	{
		var obtainableObjectInfo = obtainableObject.ObjInteractInfo.ObtainableObjectInfo;
		var effectType = obtainableObjectInfo.EffectType;
		var effectDirection = obtainableObjectInfo.PhaseDirection;

		await materialEffectController.ApplyMaterialEffectAsync(obtainableObject.ObjTransform, 
														        effectType, effectDirection, obtainDuration,
														        cancellationToken);
	}


	protected async override UniTask<bool> ProcessEnd()
	{
		// 1. 인벤토리 삽입
		var item = obtainableObject.ObjInteractInfo.ObtainableObjectInfo.EvidenceType;
		if (item) item.SetIsObtained(true);

		// 2. 오브젝트 제거
		if (obtainableObject.ObjTransform.gameObject.activeSelf)
			obtainableObject.ObjTransform.gameObject.SetActive(false);
		obtainableObject = null;
		isObtainableObjDetected = false;

		return true;
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		extraPostProcess?.Invoke();

		PlayerStatusManager.SetInterStatus(PlayerStatusManager.GetPrevInterStatus());
	}


}