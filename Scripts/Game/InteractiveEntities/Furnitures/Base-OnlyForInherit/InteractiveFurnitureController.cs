using UnityEngine;


[RequireComponent(typeof(InteractiveEntityInfo))]
public class InteractiveFurnitureController :  MonoBehaviour, IInteractiveFurniture
{
	public virtual void Interact()
	{
		throw new System.NotImplementedException();
	}
}