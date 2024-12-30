using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


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
	private static bool isMovingCoroutineOn;

	#endregion

	/// <summary>  
    /// Moving이 끝나고 'Object가 Camera 앞에 도착 / Camera 앞에서 사라지기 시작' 할 때마다 실행 -> Inspector On/Off 관리  
    /// </summary>
	public static System.Action<Transform, bool> OnObservation;
	public static bool IsMovingCoroutineOn { get => isMovingCoroutineOn; }


	#region Unity Methods

	private void Awake()
	{
        camTransformRecord = new Stack<PosRotPair>();
    }

	private void Start()
    {
        isMovingCoroutineOn = false;
    }

	private void Update()
    {
        if(!isMovingCoroutineOn)
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
        if(!isMovingCoroutineOn)
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
        //ObjectSorter.MouseHoveringObj.ObjTransform = null;

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
				StartCoroutine(StartMousePlaceObservation());

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

    private IEnumerator StartMousePlaceObservation()
    {
		var mouseObservePos = mouseObservablePlace.ObjInteractInfo.ObservablePlaceInfo.ObservingPos;
		yield return StartCoroutine(StartCamLerp(mouseObservePos.position, mouseObservePos.rotation, observeDuration));

		// [Event Invoke]: "OnWalkieTalkieDialogueStart" when interaction start with walkietalkie
		if (mouseObservablePlace.ObjType == ObjectType.WalkieTalkie && tutorialInfo.IsTutorialEnd)
			DialogueInfo.OnWalkieTalkieDialogueStart();
	}


    private void CheckEscapeForMousePlaceObservation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(EndMousePlaceObservation());
        }
        else
        if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
        {
            if (!tutorialInfo.IsTutorialEnd) return;

            StartCoroutine(EndMousePlaceObservation());
            StartCoroutine(EndPlaceObservation());
        }
    }


    private IEnumerator EndMousePlaceObservation()
    {
		// [Event Invoke]: "OnWalkieTalkieDialogueEnd" when interaction end with walkietalkie
		if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie && tutorialInfo.IsTutorialEnd)
            DialogueInfo.OnWalkieTalkieDialogueEnd();

        yield return StartCoroutine(LerpCamToPreviousPos(placeObserveDuration));

        PostProcessMousePlaceObservation();
    }

    private void PostProcessMousePlaceObservation()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

        if(mouseObservablePlace.ObjTransform.TryGetComponent<DetectiveToolInfo>(out var detectiveTool))
            if (!tutorialInfo.IsTutorialEnd)
                TutorialInfo.OnDetectiveToolTutorialed(detectiveTool.transform);
    }


    #endregion

    #region < Observable Place Interaction > - With CrossHair, On None

    /// <summary> For coming back with the most recent position </summary>
    private Stack<PosRotPair> camTransformRecord;
	private struct PosRotPair
    {
        public Vector3 pos { get; private set; }
		public Quaternion rot { get; private set; }
        public PosRotPair(Vector3 _pos, Quaternion _rot)
        {
            pos = _pos;
            rot = _rot;
		}
    }

	private float placeObserveDuration;


	//ObservablePlace -> 상호작용 시 마우스 활성화되는 Obj
	//상호작용 시 관찰하는 장소 (E로 상호작용 / 마우스로 상호작용)
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

                StartPlaceObservation();
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


	private void StartPlaceObservation()
	{
        IObjectInfo observablePlace = ObjectSorter.CHPointingObj;
        var observablePlaceInfo = observablePlace.ObjInteractInfo.ObservablePlaceInfo;

		var observePos = observablePlaceInfo.ObservingPos;
        placeObserveDuration = observablePlaceInfo.PlaceObserveDuration;

        StartCoroutine(StartCamLerp(observePos.position, observePos.rotation, placeObserveDuration));
    }

	private void CheckEscapeForPlaceObservation()
	{
        if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
        {
            StartCoroutine(EndPlaceObservation());
        }
    }

    private IEnumerator EndPlaceObservation()
	{
        yield return StartCoroutine(LerpCamToPreviousPos(placeObserveDuration));

        PostProcessPlaceObservation();
    }

    private void PostProcessPlaceObservation()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.None);
    }


    #endregion


    #region < Observable Object Interaction > 

    [Header("Observation Property")]

    [Tooltip ("Object 관찰 시, 카메라에 얼마나 가까이 위치할지 설정. Observe Object에 InteractiveEntityInfo 미부착시 적용됨")]
    [SerializeField] private float defaultZoomDistance = 0.45f;
    [Tooltip("Object 이동 시, 얼마나 느리게 이동할지 설정: 높을수록 느림 ")]
    [SerializeField] private float observeDuration = 0.5f;
    [Tooltip("Object 회전 시, 얼마나 빠르게 회전할지 설정: 높을수록 빠름")]
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
		StartCoroutine(MoveObservingObj());
	}
    private IEnumerator MoveObservingObj()
	{
        var observingObjInfo = ObservingObj.ObjInteractInfo.ObservableObjectInfo;

		float zoomDistance = observingObjInfo.ZoomDistance;
		Vector3 observePos = Camera.main.ScreenToWorldPoint(ScreenPositionGetter.GetScreenPosition(observingObjInfo.ScreenPosition, zoomDistance));

        if (observingObjInfo.IsFaceCamera)
		{
            var observeRot = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            yield return StartCoroutine(LerpObjPosRot(ObservingObj.ObjTransform, observePos, observeRot, observeDuration));
            OnObservation(ObservingObj.ObjTransform, true); //TODO(1230): OnObservation Action도 IObjectInfo로 바꿔줄 것
        }
        else
		{
            yield return StartCoroutine(LerpObjPos(ObservingObj.ObjTransform, observePos, observeDuration));
            OnObservation(ObservingObj.ObjTransform, true);
		}
        
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
    /// Key check 됐을 때 동일한 프레임으로 들어가기 때문에, 먼저 체크하거나 이전에 return 안 하면 바로 변경됨 -> 주의  
    /// </summary>
    void CheckEscapeForObservation()
	{
        if (PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.None || PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.Obtaining)
        {
            if ((HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape]))
            {
                StartCoroutine(EndObservation());
            }
        }


        else
        if (PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.Investigating)
        {
            if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(EndObservation());
            }else
            if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
            {
                if (!tutorialInfo.IsTutorialEnd) return;

                StartCoroutine(EndObservation());
                StartCoroutine(EndPlaceObservation());
            }
        }

    }



    IEnumerator EndObservation()
	{
        OnObservation(ObservingObj.ObjTransform, false);

        yield return StartCoroutine(LerpObjPosRotLocally(ObservingObj.ObjTransform, 
                                                         ObservingObj.ObjInteractInfo.ObservableObjectInfo.objLocalPos,
														 ObservingObj.ObjInteractInfo.ObservableObjectInfo.objRot, observeDuration));

        PostProcessObservation();
    }

    void PostProcessObservation()
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

    [Tooltip("Object 획득 시, 얼마나 느리게 획득할지 설정: 높을수록 느림 ")]
    [SerializeField] float obtainingDuration = 3.0f;

    /// <summary>
    /// True: 관찰 오브젝트 아래의 히든 혈흔이 화면에 보일 때, 카메라가 혈흔 가리키고 있을 때
    /// </summary>
    bool isObtainableObjDetected = false;
    IObjectInfo ObtainableObj;


    // isObtainableObjDetected -> None인 상태, Observe인 상태
    void CheckObtainmentEvent()
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

    void DetectObtainableObj()
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
           !isMovingCoroutineOn && ObservingObj.ObjTransform.childCount != 0)
		{
            // TODO(1230): 더 다듬을 수 있을 것 같음. HiddenObj 클래스 제작하지 않고 리팩토링 방법 모색할 것
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
    HiddenObj hiddenObj;

	readonly List<InteractiveEntityInfo> hinddenObjInfo = new();
    IObjectInfo GetHiddenObtObj()
    {
        hinddenObjInfo.Clear();
        hinddenObjInfo.AddRange(ObservingObj.ObjTransform.GetComponentsInChildren<InteractiveEntityInfo>(false));

        // 제일 아래에 있는 애가 숨겨진 오브젝트
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


    void StartObtainment()
	{
        iconController.ActivateIcon(iconController.FIconObj, false);
        StartCoroutine(ProcessObtainment());
    }


    IEnumerator ProcessObtainment()
    {
        var obtainableObjectInfo = ObtainableObj.ObjInteractInfo.ObtainableObjectInfo;
		var effectType = obtainableObjectInfo.EffectType;
        var effectDirection = obtainableObjectInfo.PhaseDirection;

        yield return StartCoroutine(materialEffectController.ApplyMaterialEffect(ObtainableObj.ObjTransform, effectType, effectDirection, obtainingDuration));

        PostProcessObtainment();
    }

    void PostProcessObtainment()
    {
        // 1. 인벤토리 삽입
        var item = ObtainableObj.ObjInteractInfo.ObtainableObjectInfo.EvidenceType;
        if (item)  item.SetIsObtained(true);

        // 2. 오브젝트 제거
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
    void CheckInteractionEvent()
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

	#region Pick Up & Get Back Object

        IEnumerator LerpObjPos(Transform Obj, Vector3 pickingPos, float duration)
        {
                isMovingCoroutineOn = true;


                float time = 0;
                Vector3 objPos = Obj.position;

                while (time < duration)
                {
                    Obj.position = Vector3.Lerp(objPos, pickingPos, time / duration);
                    time += Time.deltaTime;

                    yield return null;
                }
                Obj.position = pickingPos;

                isMovingCoroutineOn = false;
        }


        IEnumerator LerpObjPosRot(Transform Obj, Vector3 lerpPos, Quaternion lerpRot,  float duration)
        {
            isMovingCoroutineOn = true;


            float time = 0;
            float lerpT = time / duration;

            Vector3 objPos = Obj.position;
            Quaternion objRot = Obj.rotation;

            while (time < duration)
            {
                Obj.SetPositionAndRotation(Vector3.Lerp(objPos, lerpPos, lerpT), Quaternion.Lerp(objRot, lerpRot, lerpT));

                time += Time.deltaTime;
                lerpT = time / duration;

                yield return null;
            }
            Obj.SetPositionAndRotation(lerpPos, lerpRot);

            isMovingCoroutineOn = false;

        }


	    // lerpPos를 local로 변경, Rot는 그대로 ㅇㅇ
	    // observeObject를 돌려놓을 때

	    /// <summary>
	    /// observeObject를 돌려놓을 때 사용, LocalPos를 돌려놓아야 해서 이렇게 함 일단 나중에 분해
	    /// </summary>
	    IEnumerator LerpObjPosRotLocally(Transform Obj, Vector3 lerpLocalPos, Quaternion lerpRot, float duration)
	    {
            isMovingCoroutineOn = true;

            float time = 0;
            float lerpT = time / duration;

            Vector3 objLocalPos = Obj.localPosition;
            Quaternion objRot = Obj.rotation;

            while (time < duration)
            {
                //Obj.SetPositionAndRotation(Vector3.Lerp(objPos, lerpPos, lerpT), Quaternion.Lerp(objRot, lerpRot, lerpT));
                Obj.localPosition = Vector3.Lerp(objLocalPos, lerpLocalPos, lerpT);
                Obj.rotation = Quaternion.Lerp(objRot, lerpRot, lerpT);

                time += Time.deltaTime;
                lerpT = time / duration;

                yield return null;
            }
            //Obj.SetPositionAndRotation(lerpPos, lerpRot);
            Obj.localPosition = lerpLocalPos;
            Obj.rotation = lerpRot;


            isMovingCoroutineOn = false;
            /*
            if (forReturn)
            {
                switch (PlayerStatusManager.GetCurrentInterStatus())
                {
                    case InteractionStatus.ObservingObject:
                        PostProcessObservation();

                        break;
                    case InteractionStatus.Investigating:
                    case InteractionStatus.ObservingPlace:
                        PostProcessPlaceObservation();

                        break;
                    case InteractionStatus.Inventory:
                    case InteractionStatus.TalkingWalkieTalkie:
                        PostProcessMousePlaceObservation();

                        break;
                }
            }
            else
            {
                switch (PlayerStatusManager.GetCurrentInterStatus())
                {
                    case InteractionStatus.ObservingObject:
                        OnObservation(ObservingObj, true);

                        break;
                }
            }
            */
            // OnObservation



        }


        #endregion

    #region CamLerp

    IEnumerator StartCamLerp(Vector3 goalPos, Quaternion goalRot, float duration)
	{
        var playerCamera = Camera.main.transform;
        camTransformRecord.Push(new PosRotPair(playerCamera.position, playerCamera.rotation));

        yield return StartCoroutine(LerpObjPosRot(playerCamera, goalPos, goalRot, duration));
    }

    IEnumerator LerpCamToPreviousPos(float duration)
	{
        var playerCamera = Camera.main.transform;
        var previousCamTransform = camTransformRecord.Pop();

        yield return StartCoroutine(LerpObjPosRot(playerCamera, previousCamTransform.pos, previousCamTransform.rot, duration));

        PostProcessCamLerp();
    }
        
    void PostProcessCamLerp()
	{
        if (camTransformRecord.Count == 0)
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
    }


	#endregion


    /// <summary>
    /// 다른 곳에 옮겨도 되지 않을까? 큼
    /// </summary>
    /// <param name="objTransform"></param>
	void DisableEmissionOn(Transform objTransform)
	{
		if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
			mouseHoverEmitter.ForceStopEmitAndBlink();

		// 튜토리얼 중인데 클릭한 거면, 내려놨을 때 반짝이면 안되므로 Never로 지정.
		if (!tutorialInfo.IsTutorialEnd) mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
	}


	#endregion


}
