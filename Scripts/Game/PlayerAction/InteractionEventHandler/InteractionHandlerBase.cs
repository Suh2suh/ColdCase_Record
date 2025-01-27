using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;


public abstract class InteractionHandlerBase : IInteractionHandler
{


	public abstract bool canStartInteraction { get; }
	public async void StartInteraction(Action extraPreProcess = null)
	{
		if (canStartInteraction == false)
			return;

		bool succeed = await PreProcess(extraPreProcess);
		if (succeed)
		{ 
			ProcessStart(); 
		}
	}
	protected abstract UniTask<bool> PreProcess(Action extraPreProcess);
	protected abstract void ProcessStart();


	public abstract bool canEscapeInteraction { get; }
	public async void EndInteraction(Action extraPostProcess = null)
	{
		if (canEscapeInteraction == false)
			return;

		bool succeed = await ProcessEnd();
		if(succeed)
		{
			PostProcess(extraPostProcess);
		}
	}
	protected abstract UniTask<bool> ProcessEnd();
	protected abstract void PostProcess(Action extraPostProcess);

}