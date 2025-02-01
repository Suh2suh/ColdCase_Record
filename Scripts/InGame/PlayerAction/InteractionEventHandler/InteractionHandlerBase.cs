using Cysharp.Threading.Tasks;
using System;


public abstract class InteractionHandlerBase : IInteractionHandler
{
	private bool isOnProcess = false;

	public abstract bool canStartInteraction { get; }
	public async void StartInteraction(Action extraPreProcess = null)
	{
		if (isOnProcess || canStartInteraction == false)
			return;

		isOnProcess = true;
		bool succeed = await PreProcess(extraPreProcess);
		//if (succeed)
		//{ 
			await ProcessStart();
			isOnProcess = false;
		//}
	}
	protected abstract UniTask<bool> PreProcess(Action extraPreProcess);
	protected abstract UniTask ProcessStart();


	public abstract bool canEscapeInteraction { get; }
	public async void EndInteraction(Action extraPostProcess = null)
	{
		if (isOnProcess || canEscapeInteraction == false)
			return;

		isOnProcess = true;
		bool succeed = await ProcessEnd();
		//if(succeed)
		//{
			PostProcess(extraPostProcess);
			isOnProcess = false;
		//}
	}
	protected abstract UniTask<bool> ProcessEnd();
	protected abstract void PostProcess(Action extraPostProcess);

}