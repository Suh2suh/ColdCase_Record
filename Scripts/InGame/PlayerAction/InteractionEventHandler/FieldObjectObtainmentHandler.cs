

public class FieldObjectObtainmentHandler : ObjectObtainmentHandlerBase
{
	public override bool IsObtainableStatus
	{
		get => PlayerStatusManager.CurrentInterStatus == InteractionStatus.None ||
			   PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingPlace;
	}
	protected override IObjectInfo GetDetectedObtainableObj()
	{
		var pointingObj = (PlayerStatusManager.CurrentInterStatus == InteractionStatus.None ? ObjectSorter.CHPointingObj : ObjectSorter.MouseHoveringObj);
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