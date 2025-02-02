using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


public abstract class ObjectObtainmentHandlerBase : InteractionHandlerBase
{

	protected static ObjectInfo obtainableObject = new();
	private static bool IsObtainableObjectDetected
	{
		get => (obtainableObject.ObjTransform != null);
	}

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
			//await UniTask.WaitUntil(() => GameModeManager.CurrentGameMode == GameMode.InGame);

			if (IsObtainableStatus)
			{
				obtainableObject.Set(GetDetectedObtainableObj());
			}

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
		}
	}


	#endregion
	public abstract bool IsObtainableStatus { get; }
	/// <summary>
	/// Process obtainableObject detection here
	/// </summary>
	/// <returns></returns>
	protected abstract IObjectInfo GetDetectedObtainableObj();


	public override bool canStartInteraction
	{
		get => PhaseChecker.GetCurrentPhase() >= 'B' &&
			   IsObtainableObjectDetected &&
			   obtainableObject.ObjInteractInfo.IsInteractive;
	}
	public override bool canEscapeInteraction
	{
		get => false;   // Obtainment event is not escapable, since it's processed just for moment
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
	{
		PlayerStatusManager.SetInterStatus(InteractionStatus.Obtaining);
		extraPreProcess?.Invoke();

		return UniTask.FromResult(true);
	}


	protected override async UniTask ProcessStart()
	{
		await StartObtainmentEffectAsync();

		ForceEndInteraction();
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


	protected override UniTask<bool> ProcessEnd()
	{
		// 1. 인벤토리 삽입
		var item = obtainableObject.ObjInteractInfo.ObtainableObjectInfo.EvidenceType;
		if (item) item.SetIsObtained(true);

		// 2. 오브젝트 제거
		if (obtainableObject.ObjTransform.gameObject.activeSelf)
			obtainableObject.ObjTransform.gameObject.SetActive(false);

		return UniTask.FromResult(true);
	}


	protected override void PostProcess(Action extraPostProcess)
	{
		extraPostProcess?.Invoke();
		obtainableObject.Set(null);

		PlayerStatusManager.SetInterStatus(PlayerStatusManager.PrevInterStatus);
	}


}