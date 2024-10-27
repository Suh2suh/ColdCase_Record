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



    /// <summary> Object Sort / Ray Ȱ��ȭ ���� </summary>
    static bool isSortRayNeeded = true;

    static ObjectInfo CHpointingObj;
    public static ObjectInfo CHPointingObj  { get => CHpointingObj; }

    public static System.Action<ObjectInfo> OnCHPointingObjChanged;



    /// <summary>  Mouse Click Ray Ȱ��ȭ ����  </summary>
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
    // PC ���� �ٸ� �÷��������� UI.Select()�ϵ��� �ؾ� �� ��
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




    #region Sort Object with Mouse

    /// <summary>  Hover, Click check  </summary>
    /// Observable Place �� �ʿ�
    Transform MouseHitObj;


    /// <summary>
    /// ������ Crosshair�� ���⼭ �з��ϱ� ������, object���� onMouseEnter �� �ٿ� ���⼭ mouseEnter�ؼ� �� ���� �з��ϴ� �� �� ���ϼ� ���� ��
    ///  + desk mode������ Ȱ��ȭ�Ǿ�� �ϱ� ������, onMouseEnter�� �����ϸ� ���� �� �Ű澵 �� ����
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


    /// <summary> Conditional Inspector, Emit On Hover �� Hover �� �۵��Ǵ� ��� ��ũ��Ʈ�� ���� </summary>
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