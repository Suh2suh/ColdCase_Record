using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class CameraLerper
{
	#region Private Variables
	private Camera targetCam;
	private Stack<PosAndRot> camTransformRecord = new Stack<PosAndRot>();
	private struct PosAndRot
	{
		public Vector3 position { get; private set; }
		public Quaternion rotation { get; private set; }
		public PosAndRot(Vector3 pos, Quaternion rot)
		{
			position = pos;
			rotation = rot;
		}
	}

	#endregion

	//TODO : [250127] 추가: 현재 카메라가 lerp중인지 표시하는 bool

	public async UniTask<bool> MoveToNewTransform(Vector3 goalPos, Quaternion goalRot, float duration, CancellationToken cancellationToken)
	{
		var initialPosAndRot = new PosAndRot(targetCam.transform.position, targetCam.transform.rotation);

		bool succeed = await ObjectLerper.LerpObjTransformAsync(targetCam.transform, goalPos, goalRot, 
						       			                        duration, Space.World, cancellationToken);
		if(succeed)
		{
			camTransformRecord.Push(initialPosAndRot);
		}
		return succeed;
	}

	public async UniTask<bool> BackToPrevTransform(float duration, CancellationToken cancellationToken)
	{
		bool isPrevTransformExists = camTransformRecord.TryPeek(out var previousCamTransform);
		if (isPrevTransformExists == false)  return false;

		bool succeed = await ObjectLerper.LerpObjTransformAsync(targetCam.transform, previousCamTransform.position, previousCamTransform.rotation,
																duration, Space.World, cancellationToken);
		if (succeed)
		{
			camTransformRecord.Pop();
			PostProcessCamLerp();
		}
		return succeed;
	}	


	private void PostProcessCamLerp()
	{
		if (camTransformRecord.Count == 0)
			Camera.main.transform.localPosition = new Vector3(0, 0, 0);
	}


	#region Constructor
	public CameraLerper(Camera cam)
	{
		targetCam = cam;
	}

	#endregion

}