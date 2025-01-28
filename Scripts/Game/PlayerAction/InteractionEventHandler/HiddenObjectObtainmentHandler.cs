using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HiddenObjectObtainmentHandler : ObjectObtainmentHandlerBase
{
	private readonly List<InteractiveEntityInfo> objectLayers = new();
	private HiddenObj hiddenObj;
	private class HiddenObj : IObjectInfo
	{
		public ObjectType ObjType { get => ObjectType.ObtainableObj; }
		public Transform ObjTransform { get; set; }
		public InteractiveEntityInfo ObjInteractInfo { get; set; }
	}


	public override bool IsObtainableStatus 
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject;
	}
	protected override IObjectInfo GetDetectedObtainableObj()
	{
		if(ObjInteractor.ObservingObject?.ObjTransform.childCount <= 0)
			return null;

		// TODO: [241230] 더 다듬을 수 있을 것 같음. HiddenObj 클래스 제작하지 않고 리팩토링 방법 모색할 것
		// TODO: [250128] ObservingObj종류가 달라질 때에만 한 번 clear/addRange할 것
		objectLayers.Clear();
		objectLayers.AddRange(ObjInteractor.ObservingObject.ObjTransform.GetComponentsInChildren<InteractiveEntityInfo>(false));

		if (objectLayers.Count == 2)
		{
			InteractiveEntityInfo hiddenObjInfo = objectLayers[1];
			if (hiddenObjInfo && hiddenObjInfo.ObjectType == ObjectType.ObtainableObj)
			{
				hiddenObj.ObjInteractInfo = hiddenObjInfo;
				hiddenObj.ObjTransform = hiddenObjInfo.transform;

				return hiddenObj;
			}
		}

		return null;
	}


}