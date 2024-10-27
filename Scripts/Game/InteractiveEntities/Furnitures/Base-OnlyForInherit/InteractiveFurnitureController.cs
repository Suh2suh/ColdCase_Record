using UnityEngine;


[RequireComponent(typeof(InteractiveEntityInfo))]
public class InteractiveFurnitureController :  MonoBehaviour, IInteractiveFurniture
{
	//protected bool isInteracted;
	//public bool IsInteracted { get => isInteracted; }

	public virtual void Interact()
	{
		throw new System.NotImplementedException();
	}
}