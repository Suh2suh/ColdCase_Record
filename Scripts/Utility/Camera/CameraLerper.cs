using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class CameraLerper
{

	#region Private Variables
	private Camera targetCam;
	private bool isCameraLerping;
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
	public bool IsCameraLerping { get => isCameraLerping; }


	public async UniTask<bool> MoveToNewTransform(Vector3 goalPos, Quaternion goalRot, float duration, CancellationToken cancellationToken)
	{
		if (isCameraLerping)
			return false;
		isCameraLerping = true;

		var initialPosAndRot = new PosAndRot(targetCam.transform.position, targetCam.transform.rotation);
		bool succeed = await ObjectLerper.LerpObjTransformAsync(targetCam.transform, goalPos, goalRot, 
						       			                        duration, Space.World, cancellationToken);
		if(succeed)
		{
			isCameraLerping = false;

			camTransformRecord.Push(initialPosAndRot);
		}
		return succeed;
	}

	public async UniTask<bool> BackToPrevTransform(float duration, CancellationToken cancellationToken)
	{
		if (isCameraLerping)
			return false;
		isCameraLerping = true;

		bool isPrevTransformExists = camTransformRecord.TryPeek(out var previousCamTransform);
		if (isPrevTransformExists == false)  return false;

		bool succeed = await ObjectLerper.LerpObjTransformAsync(targetCam.transform, previousCamTransform.position, previousCamTransform.rotation,
																duration, Space.World, cancellationToken);
		if (succeed)
		{
			isCameraLerping = false;

			camTransformRecord.Pop();
			if (camTransformRecord.Count == 0)
				Camera.main.transform.localPosition = new Vector3(0, 0, 0);
		}
		return succeed;
	}	


	#region Constructor
	public CameraLerper(Camera cam)
	{
		targetCam = cam;
	}

	#endregion

}