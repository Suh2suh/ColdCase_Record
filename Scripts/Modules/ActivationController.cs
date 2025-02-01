using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationController : MonoBehaviour
{
	/// <summary>  When the Object's Activation should be managed by many classes
	///						 -> hard to manage, so attatch this one to keep it center-controlled  </summary>
	public void ActivateObj(bool activeStatus)
	{
		ActivateObj(this.gameObject, activeStatus);
	}

	public static void ActivateObj(GameObject targetObject, bool activeStatus)
	{
		if (targetObject.activeSelf != activeStatus) targetObject.SetActive(activeStatus);
	}

}
