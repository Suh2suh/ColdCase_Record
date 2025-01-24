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
    bool isLerpEventOn = false;
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
	}

	private void Start()
    {
		isLerpEventOn = false;
    }

	private void Update()
    {
        if(!isLerpEventOn)
		{
			CheckObjectObservationEvent();
            CheckPlaceObservationEvent();
            CheckMousePlaceObservationEvent();
        }

        CheckObtainmentEvent();
        CheckInteractionEvent();
        //Debug.Log(isMovingCoroutineOn + " / " + PlayerStatusManager.GetCurrentInterStatus() + " / " + ObjectSorter.CHPointingObj.objType + " / " + HotKeyChecker.isKeyPressed[HotKey.Observe]);
    }

    private void FixedUpdate()
	{
        if(!isLerpEventOn)
            RotateObjInObservation();
    }


    #endregion


    // <For All Interaction Event>
    // Check[interactionName]Event() -> Start[interactionName]()
    // -> CheckEscapeFor[interactionName]() -> End[interactionName]()
    // -> PostProcess[interactionName]()

    #region < Observable Place Interaction > -> With Mouse, On Place Observation

    private IObjectInfo mouseObservablePlace;

    private void CheckMousePlaceObservationEvent()
    {
		bool isMousePlaceObservable = PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating;
		if (isMousePlaceObservable)
        {
            var isPointingMousePlace = (ObjectSorter.MouseHoveringObj.ObjType == ObjectType.WalkieTalkie ||
                                        ObjectSorter.MouseHoveringObj.ObjType == ObjectType.Inventory);
            if (isPointingMousePlace && Input.GetMouseButtonDown(0))
            {
                // [Validate]: Mouse Observable Place
                var observablePlaceCandidate = ObjectSorter.MouseHoveringObj;

                if (observablePlaceCandidate.ObjType == ObjectType.WalkieTalkie)
                    PlayerStatusManager.SetInterStatus(InteractionStatus.TalkingWalkieTalkie);
                else
                if (observablePlaceCandidate.ObjType == ObjectType.Inventory)
                    PlayerStatusManager.SetInterStatus(InteractionStatus.Inventory);
                else
                    return;

                // [Start]: Mouse Place Observation
                mouseObservablePlace = observablePlaceCandidate;
                PreProcessMousePlaceObservation();
                StartMousePlaceObservation().Forget();

				return;
			}

        }


		bool isObservingMousePlace = PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie ||
							         PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Inventory;
		if (isObservingMousePlace)
		{
			CheckEscapeForMousePlaceObservation();
		}
	}
    

    private void PreProcessMousePlaceObservation()
    {
		DisableEmissionOn(mouseObservablePlace.ObjTransform);

		if (mouseObservablePlace.ObjTransform.TryGetComponent<PlayerCheckStatus>(out var playerCheckStatusController))
			playerCheckStatusController.SetStatusChecked();
		if (mouseObservablePlace.ObjTransform.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
			mouseHoverChecker.IsMouseHovering = false;
	}

    private async UniTaskVoid StartMousePlaceObservation()
    {
		var mouseObservePos = mouseObservablePlace.ObjInteractInfo.ObservablePlaceInfo.ObservingPos;

        isLerpEventOn = true;
		bool succeed = await plyaerCameraLerper.MoveToNewTransform(mouseObservePos.position, mouseObservePos.rotation, observeDuration,
                                                                   this.GetCancellationTokenOnDestroy());
		isLerpEventOn = false;

		// [Event Invoke]: "OnWalkieTalkieDialogueStart" when interaction start with walkietalkie
		if (succeed)
        {
			if (mouseObservablePlace.ObjType == ObjectType.WalkieTalkie && tutorialInfo.IsTutorialEnd)
				DialogueInfo.OnWalkieTalkieDialogueStart();
		}
	}


    private void CheckEscapeForMousePlaceObservation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            EndMousePlaceObservation().Forget();
        }
        else
        if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
        {
            if (!tutorialInfo.IsTutorialEnd) return;

            EndMousePlaceObservation().Forget();
            EndPlaceObservation().Forget();
        }
    }


    private async UniTaskVoid EndMousePlaceObservation()
    {
		// [Event Invoke]: "OnWalkieTalkieDialogueEnd" when interaction end with walkietalkie
		if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie && tutorialInfo.IsTutorialEnd)
            DialogueInfo.OnWalkieTalkieDialogueEnd();

        isLerpEventOn = true;
		bool succeed = await plyaerCameraLerper.BackToPrevTransform(placeObserveDuration, this.GetCancellationTokenOnDestroy());
        isLerpEventOn = false;

        if(succeed)
		    PostProcessMousePlaceObservation();
    }

    private void PostProcessMousePlaceObservation()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

        //TODO: [250124] BUGFIX
        if(mouseObservablePlace.ObjTransform.TryGetComponent<DetectiveToolInfo>(out var detectiveTool))
            if (!tutorialInfo.IsTutorialEnd)
                TutorialInfo.OnDetectiveToolTutorialed(detectiveTool.transform);
    }


    #endregion

    #region < Observable Place Interaction > - With CrossHair, On None

    /// <summary> For coming back with the most recent position </summary>

	private float placeObserveDuration;


	//ObservablePlace -> ��ȣ�ۿ� �� ���콺 Ȱ��ȭ�Ǵ� Obj
	//��ȣ�ۿ� �� �����ϴ� ��� (E�� ��ȣ�ۿ� / ���콺�� ��ȣ�ۿ�)
	private void CheckPlaceObservationEvent()
	{
        bool isPointingObservablePlace = (ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservablePlace  ||  
                                          ObjectSorter.CHPointingObj.ObjType == ObjectType.DetectiveDesk);
        bool isObservable = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None &&  isPointingObservablePlace);
        if (isObservable)
		{
            if (HotKeyChecker.isKeyPressed[HotKey.Observe])
            {
                var interactionStatus = (ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservablePlace ? 
                                         InteractionStatus.ObservingPlace : InteractionStatus.Investigating);
                PlayerStatusManager.SetInterStatus(interactionStatus);

                StartPlaceObservation().Forget();
                return;
            }
        }


        bool isObservingCHPlace = PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace ||
                                  PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating;
		if (isObservingCHPlace)
        {
			CheckEscapeForPlaceObservation();
		}
        
    }


	private async UniTaskVoid StartPlaceObservation()
	{
        IObjectInfo observablePlace = ObjectSorter.CHPointingObj;
        var observablePlaceInfo = observablePlace.ObjInteractInfo.ObservablePlaceInfo;

		var observePos = observablePlaceInfo.ObservingPos;
        placeObserveDuration = observablePlaceInfo.PlaceObserveDuration;

        isLerpEventOn = true;
		await plyaerCameraLerper.MoveToNewTransform(observePos.position, observePos.rotation, placeObserveDuration, this.GetCancellationTokenOnDestroy());
        isLerpEventOn = false;
	}

	private void CheckEscapeForPlaceObservation()
	{
        if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
        {
            EndPlaceObservation().Forget();
        }
    }

    private async UniTaskVoid EndPlaceObservation()
	{
        isLerpEventOn = true;
        bool succeed = await plyaerCameraLerper.BackToPrevTransform(placeObserveDuration, this.GetCancellationTokenOnDestroy());
        isLerpEventOn = false;

        if(succeed)
		    PostProcessPlaceObservation();
    }

    private void PostProcessPlaceObservation()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.None);
    }


    #endregion


    #region < Observable Object Interaction > 

    [Header("Observation Property")]

    [Tooltip ("Object ���� ��, ī�޶� �󸶳� ������ ��ġ���� ����. Observe Object�� InteractiveEntityInfo �̺����� �����")]
    [SerializeField] private float defaultZoomDistance = 0.45f;
    [Tooltip("Object �̵� ��, �󸶳� ������ �̵����� ����: �������� ���� ")]
    [SerializeField] private float observeDuration = 0.5f;
    [Tooltip("Object ȸ�� ��, �󸶳� ������ ȸ������ ����: �������� ����")]
    [SerializeField] private float rotSpeed = 3f;

	private IObjectInfo ObservingObj;


	private void CheckObjectObservationEvent()
	{
		if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
			CheckCHObjObservation();
		else
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating)
			CheckMouseObjObservation();

        else
		if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject)
			CheckEscapeForObservation();
	}


    private void CheckCHObjObservation()
	{
        bool isPointingObservableObj = (ObjectSorter.CHPointingObj.ObjType == ObjectType.ObservableObj);
        if (isPointingObservableObj)
		{
            if(HotKeyChecker.isKeyPressed[HotKey.Observe])
			{
                PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

                ObservingObj = ObjectSorter.CHPointingObj;

                StartObjectObservation();
			}
        }
    }
	private void CheckMouseObjObservation()
	{
		bool isPointingObservableObj = ObjectSorter.MouseHoveringObj.ObjType == ObjectType.ObservableObj;
		if (isPointingObservableObj)
		{
			if (Input.GetMouseButtonDown(0))
			{
				PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

                ObservingObj = ObjectSorter.MouseHoveringObj;

				PreprocessDeskObjObservation();
				void PreprocessDeskObjObservation()
				{
					var playerCheckStatusControllers = ObservingObj.ObjTransform.GetComponentsInChildren<PlayerCheckStatus>();
					if (ObservingObj.ObjTransform.GetComponentsInChildren<PlayerCheckStatus>() != null)
						foreach (var playerCheckStatusController in playerCheckStatusControllers)
							playerCheckStatusController.SetStatusChecked();
					DisableEmissionOn(ObservingObj.ObjTransform);
				}

				StartObjectObservation();
			}
		}
	}


	private void StartObjectObservation()
    {
        MoveObservingObj().Forget();
	}

    private async UniTaskVoid MoveObservingObj()
	{
        isLerpEventOn = true;

        var observingObjInfo = ObservingObj.ObjInteractInfo.ObservableObjectInfo;

		float zoomDistance = observingObjInfo.ZoomDistance;
		Vector3 observePos = Camera.main.ScreenToWorldPoint(ScreenPositionGetter.GetScreenPosition(observingObjInfo.ScreenPosition, zoomDistance));

        bool lerpSucceed = false;
        if (observingObjInfo.IsFaceCamera)
		{
            var observeRot = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
			lerpSucceed = await ObjectLerper.LerpObjTransformAsync(ObservingObj.ObjTransform, observePos, observeRot, observeDuration, Space.World,
                                                                    this.GetCancellationTokenOnDestroy());
        }
        else
		{
			lerpSucceed = await ObjectLerper.LerpObjTransformAsync(ObservingObj.ObjTransform, observePos, observeDuration, Space.World,
												                    this.GetCancellationTokenOnDestroy());
		}

		if (lerpSucceed)
			OnObservation(ObservingObj.ObjTransform, true); //TODO(1230): OnObservation Action�� IObjectInfo�� �ٲ��� ��

        isLerpEventOn = false;
	}


    private void RotateObjInObservation()
    {
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject)
        {
			if (ObservingObj.ObjInteractInfo.ObservableObjectInfo.IsRotatable) 
                RotateObjOnDrag(ObservingObj.ObjTransform, rotSpeed);
		}
    }



    /// <summary>  
    /// Key check ���� �� ������ ���������� ���� ������, ���� üũ�ϰų� ������ return �� �ϸ� �ٷ� ����� -> ����  
    /// </summary>
    private void CheckEscapeForObservation()
	{
        if (PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.None || PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.Obtaining)
        {
            if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
            {
                EndObservation().Forget();
            }
        }


        else
        if (PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.Investigating)
        {
            if (Input.GetMouseButtonDown(1))
            {
                EndObservation().Forget();
            }else
            if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
            {
                if (!tutorialInfo.IsTutorialEnd) return;

                EndObservation().Forget();
                EndPlaceObservation().Forget();
            }
        }

    }



    private async UniTaskVoid EndObservation()
	{
        OnObservation(ObservingObj.ObjTransform, false);

        isLerpEventOn = true;
		bool succeed = await ObjectLerper.LerpObjTransformAsync(ObservingObj.ObjTransform,
										                        ObservingObj.ObjInteractInfo.ObservableObjectInfo.objLocalPos,
										                        ObservingObj.ObjInteractInfo.ObservableObjectInfo.objRot,
										                        observeDuration, Space.Self, this.GetCancellationTokenOnDestroy());
        isLerpEventOn = false;

        if(succeed)
		    PostProcessObservation();
    }

    private void PostProcessObservation()
    {
        switch(PlayerStatusManager.GetPrevInterStatus())
		{
            case InteractionStatus.None:
            case InteractionStatus.Obtaining:
                PlayerStatusManager.SetInterStatus(InteractionStatus.None);

                break;
            case InteractionStatus.Investigating:
                PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

                break;
        }


        if( ! tutorialInfo.IsTutorialEnd && TutorialInfo.OnDetectiveToolTutorialed != null)
            TutorialInfo.OnDetectiveToolTutorialed(ObservingObj.ObjTransform); 

        
        ObservingObj = null;
    }


    #endregion

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
           !isLerpEventOn && ObservingObj.ObjTransform.childCount != 0)
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
        hinddenObjInfo.AddRange(ObservingObj.ObjTransform.GetComponentsInChildren<InteractiveEntityInfo>(false));

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
                var interactiveFurnitureController = interactiveFurniture.GetComponent<InteractiveFurnitureController>();

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
