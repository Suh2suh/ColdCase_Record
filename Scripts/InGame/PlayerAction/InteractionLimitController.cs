using System.Collections.Generic;
using UnityEngine;


// TODO: [250201] CursorManager -> GameManager에 소속 후 해당 클래스 정리
/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
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


		// TODO: Move to Game Initializer
		GameModeManager.CurrentGameMode = GameMode.InGame;
	}

	private void Start()
	{
		// TODO: Move to Game Initializer
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);
	}

	private void OnDestroy()
	{
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
		GameModeManager.OnGameModeUpdated -= OnGameModeUpdated;
	}


	private void Update()
	{
		// TODO: Delegate
		if(PlayerStatusManager.CurrentInterStatus == InteractionStatus.Photo)
		{
			if(PhotoMaker.isPhotoTaking) DisablePlayerMovement();
		}

	}


	#endregion


	void OnGameModeUpdated(GameMode newGameMode)
	{
		switch (newGameMode)
		{
			case GameMode.OutGame:
				playerController.UnLockCursor();

				break;
			case GameMode.Media:
				playerController.LockCursor();

				break;
			case GameMode.InGame:
				ManagePlayerInteraction();

				break;
		}
	}

	void OnInteractionStatusUpdated()
	{
		if (GameModeManager.GetCurrentGameMode() != GameMode.InGame)
			return;

		ManagePlayerInteraction();
	}


	#region [Action]: Player Interaction Management

	void ManagePlayerInteraction()
	{
		var prevInterStatus = PlayerStatusManager.PrevInterStatus;
		var currentInterStatus = PlayerStatusManager.CurrentInterStatus;


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
		if(currentInterStatus == InteractionStatus.None)
		{
			playerController.LockCursor();
			return;
		}

		if (mouseUnlockInterStatus.Contains(currentInterStatus))
			playerController.UnLockCursor();
	}


	#endregion


	#region [Action]: Control Player Movement

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


	#endregion


}
