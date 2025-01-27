using UnityEngine;


[RequireComponent(typeof(InteractiveEntityInfo))]
public abstract class InteractiveFurnitureBase :  MonoBehaviour, IInteractiveFurniture
{
	public virtual void Interact()
	{
		throw new System.NotImplementedException();
	}
}