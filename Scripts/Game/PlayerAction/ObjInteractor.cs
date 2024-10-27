using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
public class ObjInteractor : MonoBehaviour
{

    #region Variables: Common

    [SerializeField] IconController iconController;
    [SerializeField] MaterialEffectManager materialEffectController;
    [SerializeField] PlaceInfo placeInfo;
    [SerializeField] TutorialInfo tutorialInfo;

    static bool isMovingCoroutineOn;
    public static bool IsMovingCoroutineOn { get => isMovingCoroutineOn; }

    /// <summary>  Moving�� ������ 'Object�� Camera �տ� ���� / Camera �տ��� ������� ����' �� ������ ���� -> Inspector On/Off ����  </summary>
    public static System.Action<Transform, bool> OnObservation;

	#endregion


	#region Unity Methods

	private void Awake()
	{
        DialogueInfo.OnWalkieTalkieDialogueEnd += OnWalkieTalkieDialogueEnd;

        originalCamPos = new();
        originalCamRot = new();
    }

	private void Start()
    {
		//PlayerStatusManager.SetInterStatus(InteractionStatus.None);
        isMovingCoroutineOn = false;
    }

	private void Update()
    {
        if(!isMovingCoroutineOn)
		{
            CheckObservationEvent();
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

	private void OnDestroy()
	{
        DialogueInfo.OnWalkieTalkieDialogueEnd -= OnWalkieTalkieDialogueEnd;
    }


	#endregion




    #region < Observable Place Interaction > -> With Mouse, On Place Observation

    Transform mouseObservablePlace;

    void CheckMousePlaceObservationEvent()
    {
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie ||
            PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Inventory)
            CheckEscapeForMousePlaceObservation();


        else
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating)
        {
            var isPlaceObservable = (ObjectSorter.MouseHoveringObj.objType == ObjectType.WalkieTalkie ||
                                                     ObjectSorter.MouseHoveringObj.objType == ObjectType.Inventory);
            if (isPlaceObservable && Input.GetMouseButtonDown(0))
            {
                var observablePlaceCandidate = ObjectSorter.MouseHoveringObj;

                if (observablePlaceCandidate.objType == ObjectType.WalkieTalkie)
                    PlayerStatusManager.SetInterStatus(InteractionStatus.TalkingWalkieTalkie);
                else
                if (observablePlaceCandidate.objType == ObjectType.Inventory)
                    PlayerStatusManager.SetInterStatus(InteractionStatus.Inventory);


                mouseObservablePlace = observablePlaceCandidate.objTransform;

                CutHoverAndEmitFromObj(mouseObservablePlace);
                if (mouseObservablePlace.TryGetComponent<PlayerCheckStatus>(out var playerCheckStatusController))
                    playerCheckStatusController.SetStatusChecked();


                StartCoroutine(StartMousePlaceObservation());
            }
        }
    }

    IEnumerator StartMousePlaceObservation()
    {
        if (mouseObservablePlace.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
            mouseHoverChecker.IsMouseHovering = false;

        mouseObservablePlace.TryGetComponent<InteractiveEntityInfo>(out var interactiveEntityInfo);
        var observePos = interactiveEntityInfo.ObservablePlaceInfo.ObservingPos;
        yield return StartCoroutine(StartCamLerp(observePos.position, observePos.rotation, observeDuration));


        if (interactiveEntityInfo.ObjectType == ObjectType.WalkieTalkie && tutorialInfo.IsTutorialEnd)
            DialogueInfo.OnWalkieTalkieDialogueStart();
    }



    void CheckEscapeForMousePlaceObservation()
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
            //PopOriginalCamPosRot();
            StartCoroutine(EndPlaceObservation());
        }
    }



    IEnumerator EndMousePlaceObservation()
    {
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingWalkieTalkie &&
            tutorialInfo.IsTutorialEnd)
		{
            DialogueInfo.OnWalkieTalkieDialogueEnd();
        }
        else
		{
            yield return StartCoroutine(LerpCamToPreviousPos(placeObserveDuration));
            PostProcessMousePlaceObservation();
        }
    }


    void PostProcessMousePlaceObservation()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.Investigating);

        if(mouseObservablePlace.TryGetComponent<DetectiveToolInfo>(out var detectiveTool))
            if (!tutorialInfo.IsTutorialEnd)
                TutorialInfo.OnDetectiveToolTutorialed(detectiveTool.transform);
    }


    void OnWalkieTalkieDialogueEnd()
    {
        StartCoroutine(LerpCamToPreviousPos(placeObserveDuration));
        PostProcessMousePlaceObservation();
    }



    #endregion

    // CrossHair -> Mouse On_ Camera GO, Observable Place�� ������ None ���·� ����.
    #region < Observable Place Interaction > - With CrossHair, On None

    // Check~Event()
    // Start~
    // CheckEscapeFor~
    // End~
    // PostProcess~

    /// <summary> Cam Come back with the most recent position </summary>
    List<Vector3> originalCamPos;
    /// <summary> Cam Come back with the most recent rotation </summary>
    List<Quaternion> originalCamRot;
    float placeObserveDuration;
    // Observe Place with E or Mouse

    //ObservablePlace -> ��ȣ�ۿ� �� ���콺 Ȱ��ȭ�Ǵ� Obj
    //��ȣ�ۿ� �� �����ϴ� ��� (E�� ��ȣ�ۿ� / ���콺�� ��ȣ�ۿ�)
    void CheckPlaceObservationEvent()
	{
        bool isPointingObservablePlace = (ObjectSorter.CHPointingObj.objType == ObjectType.ObservablePlace  ||  
                                                                ObjectSorter.CHPointingObj.objType == ObjectType.DetectiveDesk);
        bool isObservable = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None) &&  isPointingObservablePlace;
        if (isObservable)
		{
            if (HotKeyChecker.isKeyPressed[HotKey.Observe])
            {
                var interactionStatus = (ObjectSorter.CHPointingObj.objType == ObjectType.ObservablePlace ? InteractionStatus.ObservingPlace : InteractionStatus.Investigating);
                PlayerStatusManager.SetInterStatus(interactionStatus);

                StartPlaceObservation();
                return;
            }
        }


        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace || PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating)
            CheckEscapeForPlaceObservation();
        
    }


    void StartPlaceObservation()
	{
        var observablePlace = ObjectSorter.CHPointingObj.objTransform;
        var observePos = observablePlace.GetComponent<InteractiveEntityInfo>().ObservablePlaceInfo.ObservingPos;
        placeObserveDuration = observablePlace.GetComponent<InteractiveEntityInfo>().ObservablePlaceInfo.PlaceObserveDuration;

        StartCoroutine(StartCamLerp(observePos.position, observePos.rotation, placeObserveDuration));
    }

    void CheckEscapeForPlaceObservation()
	{
        if (HotKeyChecker.isKeyPressed[HotKey.Observe] || HotKeyChecker.isKeyPressed[HotKey.Escape])
        {
            //if (tutorialInfo.isTutorialEnd)  
            StartCoroutine(EndPlaceObservation());
        }
    }

    IEnumerator EndPlaceObservation()
	{
        yield return StartCoroutine(LerpCamToPreviousPos(placeObserveDuration));

        PostProcessPlaceObservation();
    }

    void PostProcessPlaceObservation()
    {
        //if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace ||
        //    PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating ||
        //    PlayerStatusManager.GetCurrentInterStatus() == interactio)
              PlayerStatusManager.SetInterStatus(InteractionStatus.None);
    }

    #endregion



    // CrossHair_ Camera COME
    // �θ� ������Ʈ�� ���ӵ� ��쵵 �ֱ� ������, localPosition���� �ϴ� �� ���� ����.
    #region < Observable Object Interaction > 

    [Header("Observation Property")]

    [Tooltip ("Object ���� ��, ī�޶� �󸶳� ������ ��ġ���� ����. Observe Object�� InteractiveEntityInfo �̺����� �����")]
    [SerializeField] float defaultZoomDistance = 0.45f;
    [Tooltip("Object �̵� ��, �󸶳� ������ �̵����� ����: �������� ���� ")]
    [SerializeField] float observeDuration = 0.5f;
    [Tooltip("Object ȸ�� ��, �󸶳� ������ ȸ������ ����: �������� ����")]
    [SerializeField] float rotSpeed = 3f;

    Transform ObservingObj;
    Vector3 originalObservingObjPos;


    void CheckObservationEvent()
	{
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject)
            CheckEscapeForObservation();


        else
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
            CheckCommonObjObservation();
        else
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating)
            CheckDeskObjObservation();
    }



    void CheckDeskObjObservation()
	{
        //Debug.Log(ObjectSorter.MouseHoveringObj.objType);

        var isObjObservable =  ObjectSorter.MouseHoveringObj.objType == ObjectType.ObservableObj;
        if(isObjObservable)
		{
            if(Input.GetMouseButtonDown(0))
			{
                PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

                ObservingObj = ObjectSorter.MouseHoveringObj.objTransform;


                // TODO: �ڵ� ����
                var playerCheckStatusControllers = ObservingObj.GetComponentsInChildren<PlayerCheckStatus>();
                if (ObservingObj.GetComponentsInChildren<PlayerCheckStatus>() != null)
                    foreach (var playerCheckStatusController in playerCheckStatusControllers) playerCheckStatusController.SetStatusChecked();


                // mouse checker -> deteective desk�� �ű��� ��� ��
                CutHoverAndEmitFromObj(ObservingObj);


                StartCoroutine(MoveObservingObj());
            }
		}
	}

    void CheckCommonObjObservation()
	{
        var isObjObservable = (ObjectSorter.CHPointingObj.objType == ObjectType.ObservableObj);

        if (isObjObservable)
		{
            if(HotKeyChecker.isKeyPressed[HotKey.Observe])
			{
                PlayerStatusManager.SetInterStatus(InteractionStatus.ObservingObject);

                ObservingObj = ObjectSorter.CHPointingObj.objTransform;

                StartCoroutine(MoveObservingObj());
            }
        }
    }


    IEnumerator MoveObservingObj()
	{
        var objectInfo = ObservingObj.GetComponent<InteractiveEntityInfo>();

        var zoomDistance = (objectInfo ? objectInfo.ObservableObjectInfo.ZoomDistance : defaultZoomDistance);
        Vector3 observePos = Camera.main.ScreenToWorldPoint(ScreenPositionGetter.GetScreenPosition(objectInfo.ObservableObjectInfo.ScreenPosition, zoomDistance));

        originalObservingObjPos = ObservingObj.transform.position;
        if (objectInfo.ObservableObjectInfo.IsFaceCamera)
		{
            var observeRot = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            yield return StartCoroutine(LerpObjPosRot(ObservingObj, observePos, observeRot, observeDuration));
            OnObservation(ObservingObj, true);
        }
        else
		{
            yield return StartCoroutine(LerpObjPos(ObservingObj, observePos, observeDuration));
            OnObservation(ObservingObj, true);
		}
        
    }


    void RotateObjInObservation()
    {
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject)
        {
            if (ObservingObj.TryGetComponent<InteractiveEntityInfo>(out var objectInfo))
            {
                if (objectInfo.ObservableObjectInfo.IsRotatable) RotateObjOnDrag(ObservingObj, rotSpeed);
            }
        }
    }



    /// <summary>  Key check ���� �� ������ ���������� ���� ������, ���� üũ�ϰų� ������ return �� �ϸ� �ٷ� ����� -> ����  </summary>
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
        OnObservation(ObservingObj, false);

        var obsvObjInfo = ObservingObj.GetComponent<InteractiveEntityInfo>();
        yield return StartCoroutine(LerpObjPosRotLocally(ObservingObj, obsvObjInfo.ObservableObjectInfo.objLocalPos, obsvObjInfo.ObservableObjectInfo.objRot, observeDuration));

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
            TutorialInfo.OnDetectiveToolTutorialed(ObservingObj); 

        
        ObservingObj = null;
    }


    #endregion

    // CrossHair, Mouse
    #region < Obtainable Object Interaction >

    [Header("Obtainment Property")]

    [Tooltip("Object ȹ�� ��, �󸶳� ������ ȹ������ ����: �������� ���� ")]
    [SerializeField] float obtainingDuration = 3.0f;

    /// <summary>
    /// True: ���� ������Ʈ �Ʒ��� ���� ������ ȭ�鿡 ���� ��, ī�޶� ���� ����Ű�� ���� ��
    /// </summary>
    bool isObtainableObjDetected = false;
    Transform ObtainableObj = null;


    // isObtainableObjDetected -> None�� ����, Observe�� ����
    void CheckObtainmentEvent()
    {
        var isPlayerOnObtaiableStatus = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ||
                                                PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject ||
                                                PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingPlace);
        if (isPlayerOnObtaiableStatus)
        {
            DetectObtainableObj();

            // �̰� Mouse
            if (isObtainableObjDetected)
            {
                if (PlayerStatusManager.GetCurrentInterStatus() != InteractionStatus.None)
                    iconController.DisplayIconOnObj(iconController.FIconObj, ObtainableObj);

                bool isObtainablePhase = PhaseChecker.GetCurrentPhase() >= 'B';
                if ( ! isObtainablePhase || ! ObtainableObj.GetComponent<InteractiveEntityInfo>().IsInteractive)
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
            var obtainableObjCandidate = (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None ? ObjectSorter.CHPointingObj : ObjectSorter.MouseHoveringObj);

            if (obtainableObjCandidate.objType == ObjectType.ObtainableObj)
            {
                ObtainableObj = obtainableObjCandidate.objTransform;

                isObtainableObjDetected = true;
            }
            else
			{
                isObtainableObjDetected = false;
            }
        }
        else
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.ObservingObject && !isMovingCoroutineOn && ObservingObj.childCount != 0)
		{
            ObtainableObj = GetHiddenObtObj();

            if (ObtainableObj)
                isObtainableObjDetected = CameraViewportObjectChecker.CheckObjSeenOnCamera(ObtainableObj);
        }
        else
        {
            isObtainableObjDetected = false;
        }
    }


    readonly List<InteractiveEntityInfo> hinddenObjInfo = new();
    Transform GetHiddenObtObj()
    {
        // TODO: �� ����ϰ� �� �� �ִ� ��� ã��, ���߿� ���� �ɾ �� ���� �����Ű�� �� �� ���� ��

        hinddenObjInfo.Clear();
        hinddenObjInfo.AddRange(ObservingObj.transform.GetComponentsInChildren<InteractiveEntityInfo>(false));

        // ���� �� ó�� �� ���� �ʿ� ����, ���� �Ʒ��� �ִ� �ְ� ������ ������Ʈ��.
        if (hinddenObjInfo.Count == 2)
        {
            // hinddenObjInfo.RemoveAt(0);

            //InteractiveEntityInfo HiddenObj = hinddenObjInfo[0];
            InteractiveEntityInfo HiddenObj = hinddenObjInfo[1];
            if (HiddenObj && HiddenObj.ObjectType == ObjectType.ObtainableObj)
                return HiddenObj.transform;
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
        var objInfo = ObtainableObj.GetComponent<InteractiveEntityInfo>();
        var effectType = objInfo.ObtainableObjectInfo.EffectType;
        var effectDirection = objInfo.ObtainableObjectInfo.PhaseDirection;

        yield return StartCoroutine(materialEffectController.ApplyMaterialEffect(ObtainableObj, effectType, effectDirection, obtainingDuration));

        PostProcessObtainment();
    }

    void PostProcessObtainment()
    {
        // 1. �κ��丮 ����
        var item = ObtainableObj.GetComponent<InteractiveEntityInfo>().ObtainableObjectInfo.EvidenceType;
        if (item)  item.SetIsObtained(true);

        // 2. ������Ʈ ����
        if(ObtainableObj.gameObject.activeSelf) ObtainableObj.gameObject.SetActive(false);
        ObtainableObj = null;
        isObtainableObjDetected = false;

        PlayerStatusManager.SetInterStatus(PlayerStatusManager.GetPrevInterStatus());
    }

    #endregion




    #region Interactive Furniture Interaction

    // Check~Event()
    // Start~
    void CheckInteractionEvent()
	{
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
		{
            var isFurnitureInteractive = (ObjectSorter.CHPointingObj.objType == ObjectType.InteractiveFurniture);

            if (isFurnitureInteractive && HotKeyChecker.isKeyPressed[HotKey.Interact])
            {
                var interactiveFurniture = ObjectSorter.CHPointingObj.objTransform;
                var interactiveFurnitureController = interactiveFurniture.GetComponent<InteractiveFurnitureController>();

                interactiveFurnitureController.Interact();
            }
        }

    }


    #endregion




    #region < Common Object Interaction >


    #region Rotate Object

        public static void RotateObjOnDrag(Transform Obj, float rotSpeed)
        {
            if (Input.GetMouseButton(0))
            {
                float rotX = Input.GetAxis("Mouse X") * rotSpeed;
                float rotY = Input.GetAxis("Mouse Y") * rotSpeed;

                //Vector3 horizontalVec = -Camera.main.transform.up;
                Vector3 horizontalVec = Vector3.down;
                //Vector3 verticalVec = Camera.main.transform.right;
                Vector3 verticalVec = Vector3.right;

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


            // if prevStatus == investigating, currentStatus == observing -> ���� object�� ui ǥ��
            /*
            if(!forReturn)
		    {
                switch (PlayerStatusManager.GetCurrentInterStatus())
                {
                    case InteractionStatus.ObservingObject:
                        OnObservation(ObservingObj, true);

                        break;
                }
            }*/


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
                case InteractionStatus.TalkingWalkieTalkie:
                    if(tutorialInfo.isTutorialEnd)
                        DialogueInfo.OnWalkieTalkieDialogueStart();

                    break;
            }
        }
        */

        // OnObservation


    }


    // lerpPos�� local�� ����, Rot�� �״�� ����
    // observeObject�� �������� ��

    /// <summary>
    /// observeObject�� �������� �� ���, LocalPos�� �������ƾ� �ؼ� �̷��� �� �ϴ� ���߿� ����
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

        originalCamPos.Add(playerCamera.position);
        originalCamRot.Add(playerCamera.rotation);


        yield return StartCoroutine(LerpObjPosRot(playerCamera, goalPos, goalRot, duration));
    }

    IEnumerator LerpCamToPreviousPos(float duration)
	{
        var playerCamera = Camera.main.transform;
        //StartCoroutine(LerpObjPosRot(playerCamera, originalCamPos[^1], originalCamRot[^1], duration, forReturn: true));
        yield return StartCoroutine(LerpObjPosRot(playerCamera, originalCamPos[^1], originalCamRot[^1], duration));

        PostProcessCamLerp();
    }
        
    void PostProcessCamLerp()
	{
        PopOriginalCamPosRot();

        if (originalCamPos.Count == 0)
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
    }

    void PopOriginalCamPosRot()
	{
        if (originalCamPos.Count < 0 || originalCamRot.Count < 0)   return;

        //Debug.Log("Pos: " + originalCamPos.Count);
        //Debug.Log("Rot: " + originalCamRot.Count);

        originalCamPos.RemoveAt(originalCamPos.Count - 1);
        originalCamRot.RemoveAt(originalCamRot.Count - 1);
    }


    #endregion



    #endregion


    void CutHoverAndEmitFromObj(Transform objTransform)
    {
        if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
            mouseHoverEmitter.ForceStopEmitAndBlink();


        // Ʃ�丮�� ���ε� Ŭ���� �Ÿ�, �������� �� ��¦�̸� �ȵǹǷ� Never�� ����.
        if (!tutorialInfo.IsTutorialEnd) mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
    }


}
