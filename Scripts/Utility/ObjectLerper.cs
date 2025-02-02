using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;


public static class ObjectLerper
{
	private static Dictionary<Transform, bool> isLerpOnProcess = new Dictionary<Transform, bool>();

	private static bool IsLerpOnProcess(Transform transform)
	{
		if (isLerpOnProcess.ContainsKey(transform))
			return isLerpOnProcess[transform];
		else
			return false;
	}


	public static async UniTask<bool> LerpObjTransformAsync(Transform objTransform, Vector3 goalPos, float duration, Space space,
		                                              CancellationToken cancellationToken)
	{
		if (IsLerpOnProcess(objTransform))
			return false;

		isLerpOnProcess[objTransform] = true;
		float time = 0;
		Vector3 initialPos = (space == Space.World ? objTransform.position : objTransform.localPosition);

		while(time < duration)
		{
			objTransform.SetPosition(Vector3.Lerp(initialPos, goalPos, time / duration), space);
			time += Time.deltaTime;

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
		}

		objTransform.SetPosition(goalPos, space);
		isLerpOnProcess[objTransform] = false;

		return true;
	}
	public static async UniTask<bool> LerpObjTransformAsync(Transform objTransform, Vector3 goalPos, Quaternion goalRot, float duration, Space space,
		                                              CancellationToken cancellationToken)
	{
		if (IsLerpOnProcess(objTransform))
			return false;

		isLerpOnProcess[objTransform] = true;
		float time = 0;
		float lerpT = time / duration;
		Vector3 initialPos = (space == Space.World ? objTransform.position : objTransform.localPosition);
		Quaternion initialRot = (space == Space.World ? objTransform.rotation : objTransform.localRotation);

		while (time < duration)
		{
			objTransform.SetPositionAndRotation(Vector3.Lerp(initialPos, goalPos, lerpT), Quaternion.Lerp(initialRot, goalRot, lerpT), space);
			time += Time.deltaTime;
			lerpT = time / duration;

			//Debug.Log(objTransform.name + " Lerping... " + lerpT);

			await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
		}

		objTransform.SetPositionAndRotation(goalPos, goalRot, space);

		isLerpOnProcess[objTransform] = false;
		return true;
	}


}