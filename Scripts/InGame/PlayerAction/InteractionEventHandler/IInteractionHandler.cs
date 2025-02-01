using Cysharp.Threading.Tasks;
using System;


public interface IInteractionHandler
{
	//public void CheckEvent();

	public bool canStartInteraction { get; }
	public void StartInteraction(Action extraPreProcess = null);

	public bool canEscapeInteraction { get; }
	public void EndInteraction(Action extraPostProcess = null);

}