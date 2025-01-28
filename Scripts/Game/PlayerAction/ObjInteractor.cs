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

	// TODO: [250128] isLerpEventOn 필요성 고려 및 제거 시도할 것
	#region Private Variables

	//private bool isLerpEventOn = false;
	private CameraLerper playerCameraLerper;

	#endregion

	/// <summary>  
    /// Moving이 끝나고 'Object가 Camera 앞에 도착 / Camera 앞에서 사라지기 시작' 할 때마다 실행 -> Inspector On/Off 관리  
    /// </summary>
	public static System.Action<Transform, bool> OnObservation;
	public static IObjectInfo ObservingPlace;
	public static IObjectInfo ObservingObject;
	public static IObjectInfo ObtainingObject;


	#region Unity Methods

	private void Awake()
	{
        playerCameraLerper = new CameraLerper(Camera.main);

        Initialize(objectMouseObservationHandler);
        Initialize(objectCHObservationHandler);
        Initialize(placeMouseObservationHandler);
        Initialize(placeCHObservationHandler);

	}

	private void Start()
    {
		//isLerpEventOn = false;
    }

	private void Update()
    {
        //if(!isLerpEventOn)
		//{
            ManageObjectObservation();
            ManagePlaceObservation();
		//}

		ManageObjectObtainment();
		ManageFurnitureInteraction();

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
                (interactionHandler as PlaceMouseObservationHandler).Initialize(playerCameraLerper, tutorialInfo, this.GetCancellationTokenOnDestroy());
                break;
            case PlaceCHObservationHandler:
				(interactionHandler as PlaceCHObservationHandler).Initialize(playerCameraLerper, this.GetCancellationTokenOnDestroy());

                break;


			case ObjectObtainmentHandlerBase:
				(interactionHandler as ObjectObtainmentHandlerBase).Initialize(obtainingDuration, materialEffectController, this.GetCancellationTokenOnDestroy());

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
    [Tooltip("Object 이동 시, 얼마나 느리게 이동할지 설정: 높을수록 느림 ")]
    [SerializeField] private float observeDuration = 0.5f;
    [Tooltip("Object 회전 시, 얼마나 빠르게 회전할지 설정: 높을수록 빠름")]
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


	// TODO : [250128] Deprecated - IconController 관리 필요
	#region < Obtainable Object Interaction >

	[Header("Obtainment Property")]

    [Tooltip("Object 획득 시, 얼마나 느리게 획득할지 설정: 높을수록 느림 ")]
    [SerializeField] private float obtainingDuration = 3.0f;
	

	private FieldObjectObtainmentHandler fieldObjectObtainmentHandler = new();
	private HiddenObjectObtainmentHandler hiddenObjectObtainmentHandler = new();


	private void ManageObjectObtainment()
	{
		// TODO: [250128] 맥락적으로 고치는 것 고려 (interactor의 start조건이 같다는 것 명시할 필요 O)
		if (fieldObjectObtainmentHandler.canStartInteraction && HotKeyChecker.isKeyPressed[HotKey.Obtain])
		{
			if (fieldObjectObtainmentHandler.IsObtainableStatus)
			{
				fieldObjectObtainmentHandler.StartInteraction();
			}
			else
			if (hiddenObjectObtainmentHandler.IsObtainableStatus)
			{
				hiddenObjectObtainmentHandler.StartInteraction(() => iconController.ActivateIcon(iconController.FIconObj, false));
			}
		}
	}


		#region Deprecated
	/*
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
				// TODO: [250128] HiddenObjectObtainmentHandler에서 detect정보 불러올 것
				//if (PlayerStatusManager.GetCurrentInterStatus() != InteractionStatus.None)
					iconController.DisplayIconOnObj(iconController.FIconObj, ObtainableObj.ObjTransform);


				if (HotKeyChecker.isKeyPressed[HotKey.Obtain])
				{
					PlayerStatusManager.SetInterStatus(InteractionStatus.Obtaining);

					StartObtainment();
				}
			}
			else
			{
				// TODO: [250128] HiddenObjectObtainmentHandler에서 detect정보 불러올 것
				//iconController.ActivateIcon(iconController.FIconObj, false);
			}
		}

	}
	*/
	/*
	private void StartObtainment()
	{
		// TODO: [250128] HiddenObj.ExtraPreprocess에 삽입할 것
		iconController.ActivateIcon(iconController.FIconObj, false);
		StartCoroutine(ProcessObtainment());
	}
	*/



	#endregion

	#endregion


	//여기부터 고치면됨
	#region < Interactive Furniture Interaction >

	// Check~Event()
	// Start~
	private void ManageFurnitureInteraction()
	{
        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
		{
            bool isFurnitureInteractive = (ObjectSorter.CHPointingObj.ObjType == ObjectType.InteractiveFurniture);

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
	/// 다른 곳에 옮겨도 되지 않을까? 큼
	/// </summary>
	/// <param name="objTransform"></param>
	private void DisableEmissionOn(Transform objTransform)
	{
		if (objTransform.TryGetComponent<EmitOnMouseHover>(out var mouseHoverEmitter))
			mouseHoverEmitter.ForceStopEmitAndBlink();

		// 튜토리얼 중인데 클릭한 거면, 내려놨을 때 반짝이면 안되므로 Never로 지정.
		if (!tutorialInfo.IsTutorialEnd) mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Never);
	}


	#endregion


}
