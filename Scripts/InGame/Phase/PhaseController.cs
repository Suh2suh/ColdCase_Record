using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhaseChecker))]
public class PhaseController : MonoBehaviour
{
	// TODO: ���� Place ���� Phase Control �ٸ��� �ؾ��� -> Place: Player

	// TODO: PlaceChecker �ϳ� ����, �ɸ´� place�� �ڵ����� �ҷ��� ��
	//             ���� ���� �޶����� ������, �� ���� collider ��ġ �� place ����(enter �� ����)
	[SerializeField] PlaceInfo placeInfo;
	[SerializeField] LoadNewScene newSceneLoader;
	// ����, notebookInfo�� place�� ���� �ڵ����� �ҷ����� �ϱ�

	[SerializeField] VideoPlayController_WithAudio phaseVideoPlayController;
	[SerializeField] VideoPlayController_WithSubtitle endingVideoPlayController;

	bool isOnPhaseUpdate = false;

	readonly char nullChar = '\0';

	public static System.Action<PlaceInfo> OnPhaseUpdated;


	#region Unity Methods

	private void Awake()
	{
		PlayerStatusManager.OnInteractionStatusUpdated += OnInteractionStatusUpdated;
		phaseVideoPlayController.OnVideoPlayFinished += OnPhaseVideoPlayFinished;
		endingVideoPlayController.OnVideoPlayFinished += OnEndingVideoPlayFinished;
	}

	private void Start()
	{
		InitializePhaseInNeed();
	}

	private void OnDestroy()
	{
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
		phaseVideoPlayController.OnVideoPlayFinished -= OnPhaseVideoPlayFinished;
		endingVideoPlayController.OnVideoPlayFinished -= OnEndingVideoPlayFinished;
	}

	#endregion



	void OnInteractionStatusUpdated()
	{
		var currentInterStatus = PlayerStatusManager.CurrentInterStatus;
		var prevInterStatus = PlayerStatusManager.PrevInterStatus;

		if(currentInterStatus == InteractionStatus.None && ! isOnPhaseUpdate)
		{
			switch(PhaseChecker.GetCurrentPhase())
			{
				case 'A':
					if (prevInterStatus != InteractionStatus.TalkingNpc
						 || ! IsAPhasePassable())
						return;

					break;
				case 'B':
					if ((prevInterStatus != InteractionStatus.Investigating && prevInterStatus != InteractionStatus.Inventory && prevInterStatus != InteractionStatus.ObservingObject)
						  || ! IsBPhasePassable())
						return;

					break;
				case 'C':
					if (prevInterStatus != InteractionStatus.TalkingNpc
						 || ! IsCPhasePassable())
						return;

					break;
					case 'D':
					if ((prevInterStatus != InteractionStatus.Investigating && prevInterStatus != InteractionStatus.TalkingWalkieTalkie)
						|| !IsDPhasePassable())
						return;

					break;
			}


			// TODO:����
			lock(this)
			{
				if (PhaseChecker.GetCurrentPhase() != 'D')
				{
					TryUpdatePhase();
					isOnPhaseUpdate = true;
				}
				else
				{
					GameModeManager.SetGameMode(GameMode.Media);
					endingVideoPlayController.PlayVideoWithKey("End");
					isOnPhaseUpdate = true;
					//Debug.Log("END PROCESS");

				}
			}

		}

	}

	#region Phase Pass Checker

	/// <summary>  �� Npc�� ��� ���� ������ ��� ��û���� �� -> Phase ������Ʈ  </summary>
	bool IsAPhasePassable()
	{
		//Debug.Log(placeInfo.NotebookInPlace);
		//Debug.Log(placeInfo.NotebookInPlace.AskedQuestionSubjectPerNpc);

		var askedQuestionByNpcInPlace = placeInfo.NotebookInPlace.AskedQuestionSubjectPerNpc;

		if (askedQuestionByNpcInPlace == null || askedQuestionByNpcInPlace.Keys.Count != placeInfo.AllNpcsInPlace.Count)
			return false;

		foreach (var npcQuestionPair in askedQuestionByNpcInPlace)
		{
			var questionListPerNpc = npcQuestionPair.Value.questionList;
			if ( ! questionListPerNpc.Contains("Details"))   return false;
		}

		return true;
	}
	
	/// <summary>  ��� ���Ű� �����ǰ�, Ȯ�εǾ��ٸ� -> Phase ������Ʈ  </summary>
	bool IsBPhasePassable()
	{
		foreach(var evidence in placeInfo.AllEvidencesInPlace)
		{
			if ( ! evidence.IsObtained || ! evidence.IsChecked)
				return false;
		}

		return true;
	}

	bool IsCPhasePassable()
	{
		var askedQuestionByNpcInPlace = placeInfo.NotebookInPlace.AskedQuestionSubjectPerNpc;

		foreach (var npcQuestionPair in askedQuestionByNpcInPlace)
		{
			var questionListPerNpc = npcQuestionPair.Value.questionList;
			if(questionListPerNpc.Count >= (placeInfo.AllEvidencesInPlace.Count + DialogueInfo.basicInvestigationQuestions.Count))
				return true;
		}

		return false;
	}

	bool IsDPhasePassable()
	{
		if (placeInfo.isPlaceCleared) return true;
		else return false;
	}


	#endregion

	// ������ ���� ���� -> ���� ������ ������ ������Ʈ
	void TryUpdatePhase()
	{
		GameModeManager.SetGameMode(GameMode.Media);
		phaseVideoPlayController.PlayVideoWithKey(PhaseChecker.GetNextPhase().ToString());
	}



	void OnPhaseVideoPlayFinished()
	{
		UpdatePhase();
		GameModeManager.SetGameMode(GameMode.InGame);
	}

	void UpdatePhase()
	{
		Debug.Log("Phase Updated: " + PhaseChecker.GetNextPhase());
		placeInfo.SetPhase(PhaseChecker.GetNextPhase());

		isOnPhaseUpdate = false;
		if (OnPhaseUpdated != null) OnPhaseUpdated(placeInfo);
	}



	void OnEndingVideoPlayFinished()
	{
		GameModeManager.SetGameMode(GameMode.InGame);
		if (PlaceInfo.OnPlaceCleared != null) PlaceInfo.OnPlaceCleared(placeInfo);

		newSceneLoader.LoadANewScene("Lobby");
	}



	void InitializePhaseInNeed()
	{
		/// ONLY BEFORE BUILD: Test ������ Z�϶� �ʱ�ȭ
		if (placeInfo.Phase == nullChar || placeInfo.Phase == 'Z') TryUpdatePhase();
	}

}
