using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
/// TODO: 나중에 코드 정리 하기
public class InteractionLimitController : MonoBehaviour
{
	[SerializeField] PlayerInfo playerInfo;
	FirstPersonAIO playerController;


	#region Unity Methods

	private void Awake()
	{
		PlayerStatusManager.OnInteractionStatusUpdated += OnInteractionStatusUpdated;
		GameModeManager.OnGameModeUpdated += OnGameModeUpdated;

		playerController = this.transform.GetComponent<FirstPersonAIO>();
		isPlayerMovable = false;


		// TODO: 이건 나중에 Game Initializer로 따로 빼든지 하기
		GameModeManager.CurrentGameMode = GameMode.Game;
	}

	private void Start()
	{
		//if(playerInfo.Phase >= 'A' && playerInfo.Phase != 'Z')

		// TODO: 이건 나중에 Game Initializer로 따로 빼든지 하기
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);
	}

	private void OnDestroy()
	{
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
		GameModeManager.OnGameModeUpdated -= OnGameModeUpdated;
	}


	// 나중에 delegate로 바꾸기
	private void Update()
	{
		if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Photo)
		{
			if(PhotoMaker.isPhotoTaking) DisablePlayerMovement();
		}

	}

	#endregion


	void OnGameModeUpdated(GameMode newGameMode)
	{
		switch (newGameMode)
		{
			case GameMode.Setting:
				playerController.UnLockCursor();

				break;
			case GameMode.Media:
				playerController.LockCursor();

				break;
			case GameMode.Game:
				ManagePlayerInteraction();   // GameMode가 아닐 때 interaction status가 바뀐 경우가 있음
				                                              // ㄴ 다시 Update 해줌

				break;
		}
	}

	void OnInteractionStatusUpdated()
	{
		if (GameModeManager.GetCurrentGameMode() != GameMode.Game)   return;

		ManagePlayerInteraction();
	}

	void ManagePlayerInteraction()
	{
		var prevInterStatus = PlayerStatusManager.GetPrevInterStatus();
		var currentInterStatus = PlayerStatusManager.GetCurrentInterStatus();


		ManagePlayerMovementUnder(currentInterStatus);
		ManageRayActivationUnder(currentInterStatus);
		ManageCursorUnder(prevInterStatus, currentInterStatus);
	}



	List<InteractionStatus> playerMoveInterStatus = new() { InteractionStatus.None, InteractionStatus.Photo /*(사진 찍는 순간 빼고)*/ };
	void ManagePlayerMovementUnder(InteractionStatus currentInterStatus)
	{
		if (playerMoveInterStatus.Contains(currentInterStatus))
			EnablePlayerMovement();
		else
			DisablePlayerMovement();
	/*
		switch (currentInterStatus)
		{
			case InteractionStatus.None:
			case InteractionStatus.Photo: //(사진 찍는 순간 빼고)
				EnablePlayerMovement();

				break;
			default: // Inventory, Obtaining, Setting, Talking
				DisablePlayerMovement();

				break;
		}
	*/
	}



	void ManageRayActivationUnder(InteractionStatus currentInterStatus)
	{
		switch (currentInterStatus)
		{
			case InteractionStatus.None:
				ObjectSorter.ManageRayActivation(sortRayActiveStatus: true, mouseRayActiveStatus: false);

				break;
			case InteractionStatus.Investigating:
			case InteractionStatus.ObservingPlace:
			case InteractionStatus.Inventory:
				ObjectSorter.ManageRayActivation(sortRayActiveStatus: false, mouseRayActiveStatus: true);

				break;

			default: // Inventory, Obtaining, Setting, Talking
				ObjectSorter.ManageRayActivation(sortRayActiveStatus: false, mouseRayActiveStatus: false);

				break;
		}
	}



	List<InteractionStatus> mouseUnlockInterStatus = new() { InteractionStatus.TalkingNpc, InteractionStatus.TalkingWalkieTalkie,
																								 InteractionStatus.Investigating, InteractionStatus.ObservingPlace};
	void ManageCursorUnder(InteractionStatus prevInterStatus, InteractionStatus currentInterStatus)
	{
		//Debug.Log(prevInterStatus + " -> " + currentInterStatus);

		if(currentInterStatus == InteractionStatus.None)
			//&& mouseUnlockInterStatus.Contains(prevInterStatus))
		{
			playerController.LockCursor();
			return;
		}

		if (mouseUnlockInterStatus.Contains(currentInterStatus))
			playerController.UnLockCursor();

		/*
		switch (currentInterStatus)
		{
			case InteractionStatus.TalkingNpc:
			case InteractionStatus.TalkingWalkieTalkie:
			case InteractionStatus.Investigating:
			case InteractionStatus.ObservingPlace:
				playerController.UnLockCursor();

				break;
			default:
				playerController.LockCursor();

				break;
		}*/
	}




	bool isPlayerMovable;
	/// <summary> 카메라/캐릭터 이동 허가, 레이캐스트 재작동 </summary>
	void EnablePlayerMovement()
	{
		if (isPlayerMovable)
		{
			playerController.EnablePlayerMovement();

			isPlayerMovable = false;
		}
	}
	/// <summary> 카메라/캐릭터 이동 제한, 레이캐스트 작동 중지 </summary>
	void DisablePlayerMovement()
	{
		if (!isPlayerMovable)
		{
			playerController.DisablePlayerMovement();

			isPlayerMovable = true;
		}
	}



}
