using Cysharp.Threading.Tasks;
using UnityEngine;


/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
public class ObjInteractor : MonoBehaviour
{

    #region Setting Variables
    [SerializeField] private IconController iconController;
    [SerializeField] private MaterialEffectManager materialEffectController;
    [SerializeField] private PlaceInfo placeInfo;
    [SerializeField] private TutorialInfo tutorialInfo;

	#endregion

	private CameraLerper playerCameraLerper;

	/// <summary>  
	/// Moving이 끝나고 'Object가 Camera 앞에 도착 / Camera 앞에서 사라지기 시작' 할 때마다 실행 -> Inspector On/Off 관리  
	/// </summary>
	public static System.Action<Transform, bool> OnObservation;


	#region Unity Methods

	private void Awake()
	{
        playerCameraLerper = new CameraLerper(Camera.main);

        Initialize(objectMouseObservationHandler);
        Initialize(objectCHObservationHandler);

        Initialize(placeMouseObservationHandler);
        Initialize(placeCHObservationHandler);

		Initialize(fieldObjectObtainmentHandler);
		Initialize(hiddenObjectObtainmentHandler);
	}


	private void Update()
    {
        ManageObjectObservation();
        ManagePlaceObservation();

		ManageObjectObtainment();

		ManageFurnitureInteraction();
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
		var isOnObtaiableStatus = (PlayerStatusManager.CurrentInterStatus == InteractionStatus.None ||
								   PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingObject ||
								   PlayerStatusManager.CurrentInterStatus == InteractionStatus.ObservingPlace);
		if (isOnObtaiableStatus)
		{
			DetectObtainableObj();

			if (isObtainableObjDetected)
			{
				// TODO: [250128] HiddenObjectObtainmentHandler에서 detect정보 불러올 것
				//if (PlayerStatusManager.CurrentInterStatus != InteractionStatus.None)
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


	#region < Interactive Furniture Interaction >

	private FurnitureInteractionHandler furnitureInteractionHandler = new();
	private void ManageFurnitureInteraction()
	{
		if(furnitureInteractionHandler.canStartInteraction && HotKeyChecker.isKeyPressed[HotKey.Interact])
		{
			furnitureInteractionHandler.StartInteraction();
		}
    }


    #endregion


}
