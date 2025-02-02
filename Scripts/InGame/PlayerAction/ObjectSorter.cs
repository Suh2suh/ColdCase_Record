using UnityEngine;


public interface IObjectInfo
{
    public ObjectType ObjType { get; }
    public Transform ObjTransform { get; }
    public InteractiveEntityInfo ObjInteractInfo { get; }
}


public class ObjectSorter : MonoBehaviour
{
	private class ObjectInfo : IObjectInfo
	{
		public ObjectType ObjType { get; set; }
		public Transform ObjTransform { get; set; }
		public InteractiveEntityInfo ObjInteractInfo { get; set; }
	}


	#region Setting Variables
	[SerializeField] private float sortRayReachDistance = 1.5f;
	[SerializeField] private TutorialInfo tutorialInfo;

	#endregion

	#region Private Static Variables
	/// <summary> Object Sort / Ray Ȱ��ȭ ���� </summary>
	private static bool isSortRayNeeded = true;
    private static ObjectInfo chPointingObj;

    /// <summary>  Mouse Click Ray Ȱ��ȭ ����  </summary>
    private static bool isMouseRayNeeded;
	private static ObjectInfo mouseHoveringObj;
    
	#endregion

	public static IObjectInfo CHPointingObj { get => chPointingObj; }
	public static IObjectInfo MouseHoveringObj { get => mouseHoveringObj; }

	public static System.Action<IObjectInfo> OnCHPointingObjChanged;


    #region Unity Methods

    private void Awake()
	{
        isSortRayNeeded = true;
        chPointingObj = new();

        isMouseRayNeeded = false;
        mouseHoveringObj = new();
    }

    /*
	private void Update()
	{
        Debug.Log("CrossHair: " + CHPointingObj.objTransform + " / Mouse: " + mouseHoveringObj.objTransform);
	}
    */
	private void FixedUpdate()
	{
        // Ray �ʿ���� ���¿�����, ������ Pointing�ߴ� ��ü �״��
        if (isSortRayNeeded) SortInteractiveObjWithCH();
        else if (isMouseRayNeeded)
		{
            if (PhaseChecker.GetCurrentPhase() == 'A' && !tutorialInfo.IsTutorialEnd && !TutorialManager.IsToolHandleEnd)
                ForceInteractWithTutorialTool();

            else
                InteractWithClickableObj();
        }

        // if tutoral not end -> interactWithTutorialObj(); -> tutorialObj = tutorialManager.
    }


	#endregion


	public static void ManageRayActivation(bool sortRayActiveStatus, bool mouseRayActiveStatus)
	{
		if (isSortRayNeeded != sortRayActiveStatus) isSortRayNeeded = sortRayActiveStatus;


		if (isMouseRayNeeded != mouseRayActiveStatus)
		{
			isMouseRayNeeded = mouseRayActiveStatus;

			if (mouseRayActiveStatus == false && mouseHoveringObj != null)
				AlertHoveringOnObj(mouseHoveringObj.ObjTransform, false);
		}
	}


	#region Sort Object with Mouse

	private Transform MouseHitObj;


	/// <summary>
	/// ������ Crosshair�� ���⼭ �з��ϱ� ������, object���� onMouseEnter �� �ٿ� ���⼭ mouseEnter�ؼ� �� ���� �з��ϴ� �� �� ���ϼ� ����
	/// </summary>
	private void InteractWithClickableObj()
	{
        MouseHitObj = GetRayhitOnMouse(mouseInteractiveEntityLayer, mouseRayDistance);

        if(MouseHitObj)
		{
            if (mouseHoveringObj.ObjTransform != MouseHitObj)
			{
                var lastHoverObj = mouseHoveringObj.ObjTransform;
                AlertHoveringOnObj(lastHoverObj, false);

                var currentHoverObj = MouseHitObj.transform;
                AlertHoveringOnObj(currentHoverObj, true);


                mouseHoveringObj.ObjTransform = MouseHitObj.transform;
                mouseHoveringObj.ObjType = (MouseHitObj.transform.TryGetComponent<InteractiveEntityInfo>(out var objectInfo) ? objectInfo.ObjectType : ObjectType.None);
                mouseHoveringObj.ObjInteractInfo = objectInfo;
            }
		}
        else
        {
            if(mouseHoveringObj.ObjTransform != null)
			{
                var lastHoverObj = mouseHoveringObj.ObjTransform;
                AlertHoveringOnObj(lastHoverObj, false);

                mouseHoveringObj.ObjTransform = null;
                mouseHoveringObj.ObjType = ObjectType.None;
				mouseHoveringObj.ObjInteractInfo = null;
			}
        }

        //Debug.Log(mouseHoveringObj.objTransform + " / " + mouseHoveringObj.objType);
    }


	private void ForceInteractWithTutorialTool()
	{
        MouseHitObj = GetRayhitOnMouse(mouseInteractiveEntityLayer, mouseRayDistance);

        if (MouseHitObj && (MouseHitObj == tutorialInfo.currentTutorialTool))
		{
            var currentHoverObj = MouseHitObj.transform;
            AlertHoveringOnObj(currentHoverObj, true);

            mouseHoveringObj.ObjTransform = MouseHitObj.transform;
            mouseHoveringObj.ObjType = (MouseHitObj.transform.TryGetComponent<InteractiveEntityInfo>(out var objectInfo) ? objectInfo.ObjectType : ObjectType.None);
            mouseHoveringObj.ObjInteractInfo = objectInfo;

			if (CHPointingObj.ObjType == ObjectType.None)
				Debug.LogWarning("No script found: InteractiveEntityInfo.cs! Please put script on " + mouseHoveringObj.ObjTransform.name);
		}
        else
		{
            if (mouseHoveringObj.ObjTransform != null)
			{
                var lastHoverObj = mouseHoveringObj.ObjTransform;
                AlertHoveringOnObj(lastHoverObj, false);

                mouseHoveringObj.ObjTransform = null;
                mouseHoveringObj.ObjType = ObjectType.None;
				mouseHoveringObj.ObjInteractInfo = null;
			}
        }

    }


	/// <summary> Conditional Inspector, Emit On Hover �� Hover �� �۵��Ǵ� ��� ��ũ��Ʈ�� ���� </summary>
	private static void AlertHoveringOnObj(Transform hoveringObj  ,bool isHovering)
	{
        var targetMouseCheckers = ValidateMouseHoverChecker(hoveringObj);
        if (targetMouseCheckers == null) return;

        foreach (var mouseChecker in targetMouseCheckers)
            mouseChecker.IsMouseHovering = isHovering;
    }

	private static MouseHoverChecker[] ValidateMouseHoverChecker(Transform objTransform)
    {
        if (objTransform != null)
        {
            var mouseHoverCheckers = objTransform.GetComponents<MouseHoverChecker>();
            return mouseHoverCheckers;
        }

        return null;
    }


	#endregion

	#region Sort Object with Crosshair

	private Transform CHHitObj;
    private void SortInteractiveObjWithCH()
    {
        CHHitObj = GetRayhitOnCenter();

        if (CHHitObj)
        {
            if (CHPointingObj.ObjTransform != CHHitObj)
            {
                chPointingObj.ObjTransform = CHHitObj;
				chPointingObj.ObjType = (CHHitObj.transform.TryGetComponent<InteractiveEntityInfo>(out var objectInfo) ? objectInfo.ObjectType : ObjectType.None);
				chPointingObj.ObjInteractInfo = objectInfo;

				if(CHPointingObj.ObjType == ObjectType.None)
                    Debug.LogWarning("No script found: InteractiveEntityInfo.cs! Please put script on " + chPointingObj.ObjTransform.name);

				OnCHPointingObjChanged(chPointingObj);
            }

        }
        else
        {
            if (CHPointingObj.ObjTransform != null || CHPointingObj.ObjType != ObjectType.None)
            {
                chPointingObj.ObjTransform = null;
                chPointingObj.ObjType = ObjectType.None;
				chPointingObj.ObjInteractInfo = null;

				OnCHPointingObjChanged(chPointingObj);
            }
        }

        //Debug.Log(CHPointingObj.objTransform + " / " + CHPointingObj.objType);
    }


    #endregion


    #region Shoot Ray

    private static int crosshairInteractiveEntityLayer = 1 << 7;
	private Transform GetRayhitOnCenter()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, sortRayReachDistance, crosshairInteractiveEntityLayer))
            return hit.transform;

        return null;
    }


	private static int mouseInteractiveEntityLayer = 1 << 8;
    private static float mouseRayDistance = 100;
    public static Transform GetRayhitOnMouse(int clickableLayerMask, float rayDistance)
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayerMask))
            return hit.collider.transform;

        return null;
    }


    #endregion


}