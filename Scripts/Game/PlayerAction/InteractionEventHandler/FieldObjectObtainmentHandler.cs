using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;


public class FieldObjectObtainmentHandler : ObjectObtainmentHandlerBase
{
	public override bool IsObtainableStatus
	{
		get => PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ||
			   PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace;
	}
	protected override IObjectInfo GetDetectedObtainableObj()
	{
		var pointingObj = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ? ObjectSorter.CHPointingObj : ObjectSorter.MouseHoveringObj);
		if (pointingObj.ObjType == ObjectType.ObtainableObj)
		{
			return pointingObj;
		}
		else
		{
			return null;
		}
	}


}