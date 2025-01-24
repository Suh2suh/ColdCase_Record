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


}