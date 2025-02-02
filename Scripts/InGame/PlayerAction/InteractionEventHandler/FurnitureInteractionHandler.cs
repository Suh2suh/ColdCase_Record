using Cysharp.Threading.Tasks;
using System;


public class FurnitureInteractionHandler : InteractionHandlerBase
{
	public override bool canStartInteraction
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.None &&
			   ObjectSorter.CHPointingObj.ObjType == ObjectType.InteractiveFurniture;
	}
	public override bool canEscapeInteraction
	{
		get => false;   // since InteractiveFurniture interaction is only for moment &&
		                // it's controlled by IF child script, cannot escape!
	}


	protected override UniTask<bool> PreProcess(Action extraPreProcess)
	{
		return UniTask.FromResult(true);
	}

	protected override UniTask ProcessStart()
	{
		var interactiveFurniture = ObjectSorter.CHPointingObj.ObjTransform;
		var interactiveFurnitureController = interactiveFurniture.GetComponent<InteractiveFurnitureBase>();

		interactiveFurnitureController.Interact();

		return UniTask.CompletedTask;
	}


	protected override UniTask<bool> ProcessEnd()
	{
		return UniTask.FromResult(true);
	}
	protected override void PostProcess(Action extraPostProcess)
	{
		//throw new NotImplementedException();
	}


}