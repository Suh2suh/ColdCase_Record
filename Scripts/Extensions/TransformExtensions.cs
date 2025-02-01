using System.ComponentModel;
using UnityEngine;


public static class TransformExtensions
{

	public static void SetPosition(this Transform transform, Vector3 goalPos, [DefaultValue("Space.World")] Space space)
	{
		if (space == Space.World)
		{
			transform.position = goalPos;
		}
		else
		{
			transform.localPosition = goalPos;
		}
	}

	public static void SetPositionAndRotation(this Transform transform, Vector3 goalPos, Quaternion goalRot, [DefaultValue("Space.World")] Space space)
	{
		if (space == Space.World)
		{
			transform.position = goalPos;
			transform.rotation = goalRot;
		}
		else
		{
			transform.localPosition = goalPos;
			transform.localRotation = goalRot;
		}
	}


	

	// TODO: [250201] 더 모듈화
	public static void RotateOnDrag(this Transform transform, float rotSpeed)
	{
		if (Input.GetMouseButton(0))
		{
			float rotX = Input.GetAxis("Mouse X") * rotSpeed;
			float rotY = Input.GetAxis("Mouse Y") * rotSpeed;

			Vector3 horizontalVec = -Camera.main.transform.up;
			Vector3 verticalVec = Camera.main.transform.right;

			transform.Rotate(horizontalVec, rotX, Space.World);
			transform.Rotate(verticalVec, rotY, Space.World);
		}
	}


}