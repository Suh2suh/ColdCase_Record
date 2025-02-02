using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// POSSIBLE: For Hidden Obtainable Object, Do not set its layer to (Crosshair interactive Entity || Mouse Interactive Entity)
///           Set it to default or other!
/// </summary>
public class HiddenObjectObtainmentHandler : ObjectObtainmentHandlerBase
{
	private readonly List<InteractiveEntityInfo> objectLayers = new();
	private Transform observingObj;
	private HiddenObj hiddenObj = new();
	private class HiddenObj : IObjectInfo
	{
		public ObjectType ObjType { get => ObjectType.ObtainableObj; }
		public Transform ObjTransform { get; set; } = null;
		public InteractiveEntityInfo ObjInteractInfo { get; set; } = null;
	}


	public HiddenObjectObtainmentHandler()
	{
		ObjInteractor.OnObservation += OnObservation;
	}
	~HiddenObjectObtainmentHandler()
	{
		ObjInteractor.OnObservation -= OnObservation;
	}


	private void OnObservation(Transform newObservingObj, bool isOn)
	{
		if(isOn)
		{
			observingObj = newObservingObj;

			DetectHiddenObject();
			void DetectHiddenObject()
			{
				objectLayers.AddRange(observingObj.GetComponentsInChildren<InteractiveEntityInfo>(false));
				if (objectLayers.Count == 2)
				{
					InteractiveEntityInfo hiddenObjInfo = objectLayers[1];  // POSSIBLE: [250202] 현재는 무조건 2개 레이어만 보장하지만,
					                                                        //                    추후 2>= 레이어 오브젝트 생성 시 해당 부분 수정 필요
					if (hiddenObjInfo && hiddenObjInfo.ObjectType == ObjectType.ObtainableObj)
					{
						hiddenObj.ObjInteractInfo = hiddenObjInfo;
						hiddenObj.ObjTransform = hiddenObjInfo.transform;
					}
				}
			}
		}
		else
		{
			observingObj = null;

			hiddenObj.ObjTransform = null;
			hiddenObj.ObjInteractInfo = null;

			objectLayers.Clear();
		}
	}


	public override bool IsObtainableStatus 
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingObject;
	}
	protected override IObjectInfo GetDetectedObtainableObj()
	{
		if (hiddenObj.ObjTransform != null &&
			CameraViewportObjectChecker.CheckObjSeenOnCamera(hiddenObj.ObjTransform))
		{
			Debug.Log("발견!");

			return hiddenObj;
		}

		return null;
	}


}