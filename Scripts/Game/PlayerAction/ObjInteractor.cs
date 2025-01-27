using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: [250124]Refactoring needed
/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
public class ObjInteractor : MonoBehaviour
{

    #region Setting Variables
    [SerializeField] private IconController iconController;
    [SerializeField] private MaterialEffectManager materialEffectController;
    [SerializeField] private PlaceInfo placeInfo;
    [SerializeField] private TutorialInfo tutorialInfo;

	#endregion

	#region Private Variables
	private bool isLerpEventOn = false;
    private CameraLerper plyaerCameraLerper;

	#endregion

	/// <summary>  
    /// Moving�� ������ 'Object�� Camera �տ� ���� / Camera �տ��� ������� ����' �� ������ ���� -> Inspector On/Off ����  
    /// </summary>
	public static System.Action<Transform, bool> OnObservation;


	#region Unity Methods

	private void Awake()
	{
        plyaerCameraLerper = new CameraLerper(Camera.main);

        Initialize(objectMouseObservationHandler);
        Initialize(objectCHObservationHandler);
        Initialize(placeMouseObservationHandler);
        Initialize(placeCHObservationHandler);

	}

	private void Start()
    {
		isLerpEventOn = false;
    }

	private void Update()
    {
        if(!isLerpEventOn)
		{
            ManageObjectObservation();
            ManagePlaceObservation();
		}

        CheckObtainmentEvent();
        CheckInteractionEvent();

        //Debug.Log(isMovingCoroutineOn + " / " + PlayerStatusManager.GetCurrentInterStatus() + " / " + ObjectSorter.CHPointingObj.objType + " / " + HotKeyChecker.isKeyPressed[HotKey.Observe]);
    }


	#endregion

	#region Initializaiton

	private void Initialize(IInteractionHandler interactionHandler)
	{
		switch (interactionHandler)
		{
			case ObjectObservationHandlerBase:
				var observationHandler = (ObjectObservationHandlerBase)interactionHandler;
				observationHandler.Initialize(observeDuration, rotSpeed, this.GetCancellationTokenOnDestroy());

				if (observationHandler is ObjectMouseObservationHandler)
					(observationHandler as ObjectMouseObservationHandler).tutorialInfo = tutorialInfo;

				break;
            case PlaceMouseObservationHandler:
                (interactionHandler as PlaceMouseObservationHandler).Initialize(plyaerCameraLerper, tutorialInfo, this.GetCancellationTokenOnDestroy());

                break;
            case PlaceCHObservationHandler:
				(interactionHandler as PlaceCHObservationHandler).Initialize(plyaerCameraLerper, this.GetCancellationTokenOnDestroy());

                break;
		}
	}


	#endregion


	#region < Observable Place Interaction > 

	private PlaceMouseObservationHandler placeMouseObservationHandler = new();
	private PlaceCHObservationHandler placeCHObservationHandler = new();


	private void ManagePlaceObservation()
	{
		ManagePlaceMouseObservation();
		ManagePlaceCHObservation();
	}


	private void ManagePlaceMouseObservation()
	{
		if (placeMouseObservationHandler.canStartInteraction && Input.GetMouseButtonDown(0))
		{
			placeMouseObservationHandler.StartInteraction();
		}

		else
		if (placeMouseObservationHandler.canEscapeInteraction)
		{
			if (Input.GetMouseButtonDown(1))
			{
				placeMouseObservationHandler.EndInteraction();
			}
			else
			if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
			{
				if (!tutorialInfo.IsTutorialEnd)
					return;

				placeMouseObservationHandler.EndInteraction();
				placeCHObservationHandler.EndInteraction();
			}
		}
	}


	private void ManagePlaceCHObservation()
	{
		if (placeCHObservationHandler.canStartInteraction && HotKeyChecker.isKeyPressed[HotKey.Observe])
		{
			placeCHObservationHandler.StartInteraction();
		}

		else
		if (placeCHObservationHandler.canEscapeInteraction && (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape]))
		{
			placeCHObservationHandler.EndInteraction();
		}
	}


	#endregion

	#region < Observable Object Interaction > 

	[Header("Observation Property")]
    [Tooltip("Object �̵� ��, �󸶳� ������ �̵����� ����: �������� ���� ")]
    [SerializeField] private float observeDuration = 0.5f;
    [Tooltip("Object ȸ�� ��, �󸶳� ������ ȸ������ ����: �������� ����")]
    [SerializeField] private float rotSpeed = 3f;


    private ObjectMouseObservationHandler objectMouseObservationHandler = new ObjectMouseObservationHandler();
    private ObjectCHObservationHandler objectCHObservationHandler = new ObjectCHObservationHandler();


    private void ManageObjectObservation()
    {
        ManageObjectMouseObservation();
        ManageObjectCHObservation();
	}


    private void ManageObjectMouseObservation()
    {
		if (objectMouseObservationHandler.canStartInteraction && Input.GetMouseButtonDown(0))
        {
			objectMouseObservationHandler.StartInteraction();
		}

        else
		if (objectMouseObservationHandler.canEscapeInteraction)
		{
			if (Input.GetMouseButtonDown(1))
			{
				objectMouseObservationHandler.EndInteraction();
			}
			else
			if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
			{
				if (!tutorialInfo.IsTutorialEnd)
                    return;

				objectMouseObservationHandler.EndInteraction();
				placeCHObservationHandler.EndInteraction();
			}
		}
	}


    private void ManageObjectCHObservation()
    {
		if (objectCHObservationHandler.canStartInteraction && HotKeyChecker.isKeyPressed[HotKey.Observe])
			objectCHObservationHandler.StartInteraction();

        else
		if (objectCHObservationHandler.canEscapeInteraction && (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape]))
			objectCHObservationHandler.EndInteraction();
	}

    
    #endregion


    // ������� �����ϸ� ��
    #region < Obtainable Object Interaction >

    [Header("Obtainment Property")]

    [Tooltip("Object ȹ�� ��, �󸶳� ������ ȹ������ ����: �������� ���� ")]
    [SerializeField] private float obtainingDuration = 3.0f;

    /// <summary>
    /// True: ���� ������Ʈ �Ʒ��� ���� ������ ȭ�鿡 ���� ��, ī�޶� ���� ����Ű�� ���� ��
    /// </summary>
    private bool isObtainableObjDetected = false;
    private IObjectInfo ObtainableObj;


    // isObtainableObjDetected -> None�� ����, Observe�� ����
    private void CheckObtainmentEvent()
    {
        var isOnObtaiableStatus = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ||
                                   PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject ||
                                   PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace);
        if (isOnObtaiableStatus)
        {
            DetectObtainableObj();

            if (isObtainableObjDetected)
            {
                if (PlayerStatusManager.GetCurrentInterStatus() != InteractionStatus.None)
                    iconController.DisplayIconOnObj(iconController.FIconObj, ObtainableObj.ObjTransform);

                bool isObtainablePhase = PhaseChecker.GetCurrentPhase() >= 'B';
                if ( ! isObtainablePhase || ! ObtainableObj.ObjInteractInfo.IsInteractive)
                    return;


                if (HotKeyChecker.isKeyPressed[HotKey.Obtain])
                {
                    PlayerStatusManager.SetInterStatus(InteractionStatus.Obtaining);

                    StartObtainment();
                }
            }
            else
            {
                iconController.ActivateIcon(iconController.FIconObj, false);
            }
        }

    }

    private void DetectObtainableObj()
    {
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None || 
            PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace)
        {
            var pointingObj = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ? ObjectSorter.CHPointingObj : ObjectSorter.MouseHoveringObj);
            if (pointingObj.ObjType == ObjectType.ObtainableObj)
            {
                ObtainableObj = pointingObj;

                isObtainableObjDetected = true;
            }
            else
			{
                isObtainableObjDetected = false;
            }
        }
        else
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject &&
           !isLerpEventOn && objectCHObservationHandler.observableObject?.ObjTransform.childCount > 0)
		{
            // TODO(1230): �� �ٵ��� �� ���� �� ����. HiddenObj Ŭ���� �������� �ʰ� �����丵 ��� ����� ��
            ObtainableObj = GetHiddenObtObj();

            if (ObtainableObj != null)
                isObtainableObjDetected = CameraViewportObjectChecker.CheckObjSeenOnCamera(ObtainableObj.ObjTransform);
        }
        else
        {
            isObtainableObjDetected = false;
        }
    }


	private class HiddenObj : IObjectInfo
	{
        public ObjectType ObjType { get => ObjectType.ObtainableObj; }
		public Transform ObjTransform { get; set; }
		public InteractiveEntityInfo ObjInteractInfo { get; set; }
	}
	private HiddenObj hiddenObj;

	private readonly List<InteractiveEntityInfo> hinddenObjInfo = new();
    private IObjectInfo GetHiddenObtObj()
    {
        hinddenObjInfo.Clear();
        hinddenObjInfo.AddRange(objectCHObservationHandler.observableObject.ObjTransform.GetComponentsInChildren<InteractiveEntityInfo>(false));

        // ���� �Ʒ��� �ִ� �ְ� ������ ������Ʈ
        if (hinddenObjInfo.Count == 2)
        {
            InteractiveEntityInfo hiddenObjInfo = hinddenObjInfo[1];
            if (hiddenObjInfo && hiddenObjInfo.ObjectType == ObjectType.ObtainableObj)
            {
                hiddenObj.ObjInteractInfo = hiddenObjInfo;
				hiddenObj.ObjTransform = hiddenObjInfo.transform;

				return hiddenObj;
			}
        }

        return null;
    }


	private void StartObtainment()
	{
        iconController.ActivateIcon(iconController.FIconObj, false);
        StartCoroutine(ProcessObtainment());
    }


    private IEnumerator ProcessObtainment()
    {
        var obtainableObjectInfo = ObtainableObj.ObjInteractInfo.ObtainableObjectInfo;
		var effectType = obtainableObjectInfo.EffectType;
        var effectDirection = obtainableObjectInfo.PhaseDirection;

        yield return StartCoroutine(materialEffectController.ApplyMaterialEffect(ObtainableObj.ObjTransform, effectType, effectDirection, obtainingDuration));

        PostProcessObtainment();
    }

	private void PostProcessObtainment()
    {
        // 1. �κ��丮 ����
        var item = ObtainableObj.ObjInteractInfo.ObtainableObjectInfo.EvidenceType;
        if (item)  item.SetIsObtained(true);

        // 2. ������Ʈ ����
        if(ObtainableObj.ObjTransform.gameObject.activeSelf)
            ObtainableObj.ObjTransform.gameObject.SetActive(false);
        ObtainableObj = null;
        isObtainableObjDetected = false;

        PlayerStatusManager.SetInterStatus(PlayerStatusManager.GetPrevInterStatus());
    }

	#endregion


	#region < Interactive Furniture Interaction >

	// Check~Event()
	// Start~
	private void CheckInteractionEvent()
	{
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
		{
            var isFurnitureInteractive = (ObjectSorter.CHPointingObj.ObjType == ObjectType.InteractiveFurniture);

            if (isFurnitureInteractive && HotKeyChecker.isKeyPressed[HotKey.Interact])
            {
                var interactiveFurniture = ObjectSorter.CHPointingObj.ObjTransform;
                var interactiveFurnitureController = interactiveFurniture.GetComponent<InteractiveFurnitureBase>();

                interactiveFurnitureController.Interact();
            }
        }

    }


    #endregion


    #region < Utility >


    #region Rotate Object

        public static void RotateObjOnDrag(Transform Obj, float rotSpeed)
        {
            if (Input.GetMouseButton(0))
            {
                float rotX = Input.GetAxis("Mouse X") * rotSpeed;
                float rotY = Input.GetAxis("Mouse Y") * rotSpeed;

                Vector3 horizontalVec = -Camera.main.transform.up;
                //Vector3 horizontalVec = Vector3.down;
                Vector3 verticalVec = Camera.main.transform.right;
                //Vector3 verticalVec = Vector3.right;

                Obj.transform.Rotate(horizontalVec, rotX, Space.World);
                Obj.transform.Rotate(verticalVec, rotY, Space.World);
            }
        }


	#endregion



	/// <summary>
	/// �ٸ� ���� �Űܵ� ���� ������? ŭ
	/// </summary>
	/// <param name="objTransform"></param>
	private void DisableEmissionOn(Transform objTransform)
	{
		if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
			mouseHoverEmitter.ForceStopEmitAndBlink();

		// Ʃ�丮�� ���ε� Ŭ���� �Ÿ�, �������� �� ��¦�̸� �ȵǹǷ� Never�� ����.
		if (!tutorialInfo.IsTutorialEnd) mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
	}


	#endregion


}
