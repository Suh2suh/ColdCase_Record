using System.Collections;
using UnityEngine;

public abstract class PlayerCheckStatus : MonoBehaviour
{
	protected bool isCheckedByPlayer = false;

	/// <summary> When Conditional Inspector is on / Pick Up Object With Mouse </summary>
	public virtual void SetStatusChecked()
	{
		if (isCheckedByPlayer == true) return;

		isCheckedByPlayer = true;
		OnCheckedByPlayer();
	}
	public void ResetCheckStatus()
	{
		isCheckedByPlayer = false;
	}


	// onCheckInspector() -> inspectorManager.OnInspect(Transform)
	// if Transform == this.transform -> isCheckedByPlayer


	public abstract void OnCheckedByPlayer();

}