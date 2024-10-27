using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>  Should be attatched to obj with 'FirstPersonAIO' Component(=Player)  </summary>
/// TODO: ���߿� �ڵ� ���� �ϱ�
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


		// TODO: �̰� ���߿� Game Initializer�� ���� ������ �ϱ�
		GameModeManager.CurrentGameMode = GameMode.Game;
	}

	private void Start()
	{
		//if(playerInfo.Phase >= 'A' && playerInfo.Phase != 'Z')

		// TODO: �̰� ���߿� Game Initializer�� ���� ������ �ϱ�
		PlayerStatusManager.SetInterStatus(InteractionStatus.None);
	}

	private void OnDestroy()
	{
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
		GameModeManager.OnGameModeUpdated -= OnGameModeUpdated;
	}


	// ���߿� delegate�� �ٲٱ�
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
				ManagePlayerInteraction();   // GameMode�� �ƴ� �� interaction status�� �ٲ� ��찡 ����
				                                              // �� �ٽ� Update ����

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



	List<InteractionStatus> playerMoveInterStatus = new() { InteractionStatus.None, InteractionStatus.Photo /*(���� ��� ���� ����)*/ };
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
			case InteractionStatus.Photo: //(���� ��� ���� ����)
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
	/// <summary> ī�޶�/ĳ���� �̵� �㰡, ����ĳ��Ʈ ���۵� </summary>
	void EnablePlayerMovement()
	{
		if (isPlayerMovable)
		{
			playerController.EnablePlayerMovement();

			isPlayerMovable = false;
		}
	}
	/// <summary> ī�޶�/ĳ���� �̵� ����, ����ĳ��Ʈ �۵� ���� </summary>
	void DisablePlayerMovement()
	{
		if (!isPlayerMovable)
		{
			playerController.DisablePlayerMovement();

			isPlayerMovable = true;
		}
	}



}
