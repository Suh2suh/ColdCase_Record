using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


public abstract class InteractionHandlerBase : IInteractionHandler
{
	private bool isOnProcess = false;


	public abstract bool canStartInteraction { get; }
	public async void StartInteraction(Action extraPreProcess = null)
	{
		if (isOnProcess || canStartInteraction == false)
			return;

		isOnProcess = true;
		await PreProcess(extraPreProcess);

		await ProcessStart();
		isOnProcess = false;
	}
	protected abstract UniTask<bool> PreProcess(Action extraPreProcess);
	protected abstract UniTask ProcessStart();


	public abstract bool canEscapeInteraction { get; }
	public async void EndInteraction(Action extraPostProcess = null)
	{
		if (isOnProcess || canEscapeInteraction == false)
			return;

		isOnProcess = true;
		await ProcessEnd();

		PostProcess(extraPostProcess);
		isOnProcess = false;
	}
	/// <summary>
	/// Used for handler which ends its interaction right after finishing (e.g.)Object Obtainment...
	/// </summary>
	/// <param name="extraPostProcess"></param>
	protected async void ForceEndInteraction(Action extraPostProcess = null)
	{
		isOnProcess = true;
		await ProcessEnd();

		PostProcess(extraPostProcess);
		isOnProcess = false;
	}
	protected abstract UniTask<bool> ProcessEnd();
	protected abstract void PostProcess(Action extraPostProcess);




	/// <summary>
	/// Used for saving ObjectSorter's pointing object as value type (only initialized once in preprocess, unchangable!)
	/// </summary>
	protected class ObjectInfo : IObjectInfo
	{
		public ObjectType ObjType { get; private set; }
		public Transform ObjTransform { get; private set; }
		public InteractiveEntityInfo ObjInteractInfo { get; private set; }
		public void Set(IObjectInfo objInfo)
		{
			if (objInfo == null)
			{
				ObjType = ObjectType.None;
				ObjTransform = null;
				ObjInteractInfo = null;

				return;
			}

			ObjType = objInfo.ObjType;
			ObjTransform = objInfo.ObjTransform;
			ObjInteractInfo = objInfo.ObjInteractInfo;
		}
	}

}