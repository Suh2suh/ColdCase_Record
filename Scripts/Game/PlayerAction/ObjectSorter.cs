using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectInfo
{
    public ObjectType objType;
    public Transform objTransform;
}


/// <summary> Attachable to Any script </summary>
public class ObjectSorter : MonoBehaviour
{

	#region Variables
    
    [SerializeField] float sortRayReachDistance = 1.5f;

    [SerializeField] TutorialInfo tutorialInfo;



    /// <summary> Object Sort / Ray 활성화 여부 </summary>
    static bool isSortRayNeeded = true;

    static ObjectInfo CHpointingObj;
    public static ObjectInfo CHPointingObj  { get => CHpointingObj; }

    public static System.Action<ObjectInfo> OnCHPointingObjChanged;



    /// <summary>  Mouse Click Ray 활성화 여부  </summary>
    static bool isMouseRayNeeded;
    /*
    public static bool IsMouseRayNeeded
	{
        get => isMouseRayNeeded;
        set
        {
            isMouseRayNeeded = value;

            if (value == false && mouseHoveringObj != null)
            {
                AlertHoveringOnObj(mouseHoveringObj.objTransform, false);

                mouseHoveringObj.objTransform = null;
                mouseHoveringObj.objType = ObjectType.None;
            }
        }
    }*/
    // PC 말고 다른 플랫폼에서는 UI.Select()하듯이 해야 할 듯
    static ObjectInfo mouseHoveringObj;
    public static ObjectInfo MouseHoveringObj { get => mouseHoveringObj; }

    

    public static void ManageRayActivation(bool sortRayActiveStatus, bool mouseRayActiveStatus)
	{
        if(isSortRayNeeded != sortRayActiveStatus)   isSortRayNeeded = sortRayActiveStatus;


        if(isMouseRayNeeded != mouseRayActiveStatus)
		{   
            isMouseRayNeeded = mouseRayActiveStatus;

            if(mouseRayActiveStatus == false && mouseHoveringObj != null)
			{
                AlertHoveringOnObj(mouseHoveringObj.objTransform, false);
                //mouseHoveringObj.objTransform = null;
                //mouseHoveringObj.objType = ObjectType.None;
            }
        }

    }






    #endregion


    #region Unity Methods

    private void Awake()
	{
        isSortRayNeeded = true;
        CHpointingObj = new();

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
        // Ray 필요없는 상태에서는, 직전에 Pointing했던 물체 그대로
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




    #region Sort Object with Mouse

    /// <summary>  Hover, Click check  </summary>
    /// Observable Place 중 필요
    Transform MouseHitObj;


    /// <summary>
    /// 어차피 Crosshair을 여기서 분류하기 때문에, object에서 onMouseEnter 쓸 바에 여기서 mouseEnter해서 한 번에 분류하는 게 더 통일성 있을 듯
    ///  + desk mode에서만 활성화되어야 하기 때문에, onMouseEnter로 관리하면 괜히 더 신경쓸 게 많음
    /// </summary>
    void InteractWithClickableObj()
	{
        MouseHitObj = GetRayhitOnMouse(mouseInteractiveEntityLayer, mouseRayDistance);

        if(MouseHitObj)
		{
            if (mouseHoveringObj.objTransform != MouseHitObj)
			{
                var lastHoverObj = mouseHoveringObj.objTransform;
                AlertHoveringOnObj(lastHoverObj, false);

                var currentHoverObj = MouseHitObj.transform;
                AlertHoveringOnObj(currentHoverObj, true);


                mouseHoveringObj.objTransform = MouseHitObj.transform;
                mouseHoveringObj.objType = (MouseHitObj.transform.TryGetComponent<InteractiveEntityInfo>(out var objectInfo) ? objectInfo.ObjectType : ObjectType.None);
            }
		}
        else
        {
            if(mouseHoveringObj.objTransform != null)
			{
                var lastHoverObj = mouseHoveringObj.objTransform;
                AlertHoveringOnObj(lastHoverObj, false);


                mouseHoveringObj.objTransform = null;
                mouseHoveringObj.objType = ObjectType.None;
            }
        }

        //Debug.Log(mouseHoveringObj.objTransform + " / " + mouseHoveringObj.objType);
    }


    void ForceInteractWithTutorialTool()
	{
        MouseHitObj = GetRayhitOnMouse(mouseInteractiveEntityLayer, mouseRayDistance);

        if (MouseHitObj && (MouseHitObj == tutorialInfo.currentTutorialTool))
		{
            var currentHoverObj = MouseHitObj.transform;
            AlertHoveringOnObj(currentHoverObj, true);

            mouseHoveringObj.objTransform = MouseHitObj.transform;
            mouseHoveringObj.objType = (MouseHitObj.transform.TryGetComponent<InteractiveEntityInfo>(out var objectInfo) ? objectInfo.ObjectType : ObjectType.None);
        }
        else
		{
            if (mouseHoveringObj.objTransform != null)
			{
                var lastHoverObj = mouseHoveringObj.objTransform;
                AlertHoveringOnObj(lastHoverObj, false);

                mouseHoveringObj.objTransform = null;
                mouseHoveringObj.objType = ObjectType.None;
            }
        }

    }


    /// <summary> Conditional Inspector, Emit On Hover 등 Hover 시 작동되는 모든 스크립트를 관리 </summary>
    static void AlertHoveringOnObj(Transform hoveringObj  ,bool isHovering)
	{
        var targetMouseCheckers = ValidateMouseHoverChecker(hoveringObj);
        if (targetMouseCheckers == null) return;

        foreach (var mouseChecker in targetMouseCheckers)
            mouseChecker.IsMouseHovering = isHovering;
    }

    static MouseHoverChecker[] ValidateMouseHoverChecker(Transform objTransform)
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

    Transform CHHitObj;
    void SortInteractiveObjWithCH()
    {
        CHHitObj = GetRayhitOnCenter();

        if (CHHitObj)
        {
            if (CHPointingObj.objTransform != CHHitObj)
            {
                CHpointingObj.objTransform = CHHitObj;

                if(CHHitObj.TryGetComponent<InteractiveEntityInfo>(out var objectInfo))
				{
                    CHpointingObj.objType = objectInfo.ObjectType;
                }
                else
                {
                    CHpointingObj.objType = ObjectType.None;
                    Debug.LogWarning("No script found: InteractiveEntityInfo.cs! Please put script on " + CHpointingObj.objTransform.name);
                }

                OnCHPointingObjChanged(CHpointingObj);
            }

        }
        else
        {
            if (CHPointingObj.objTransform != null || CHPointingObj.objType != ObjectType.None)
            {
                CHpointingObj.objTransform = null;
                CHpointingObj.objType = ObjectType.None;

                OnCHPointingObjChanged(CHpointingObj);
            }
        }

        //Debug.Log(CHPointingObj.objTransform + " / " + CHPointingObj.objType);
    }


    #endregion


    #region Shoot Ray


    static int crosshairInteractiveEntityLayer = 1 << 7;   // crosshair Interactive Object
    Transform GetRayhitOnCenter()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, sortRayReachDistance, crosshairInteractiveEntityLayer))
            return hit.transform;

        return null;
    }


    static int mouseInteractiveEntityLayer = 1 << 8;   // mouse Interactive Object
    static float mouseRayDistance = 100;

    /// <summary> raycast Layer => !7 </summary>
    /// <returns></returns>
    public static Transform GetRayhitOnMouse(int clickableLayerMask, float rayDistance)
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayerMask))
            return hit.collider.transform;
            //return hit.transform;

        return null;
    }


    #endregion


}