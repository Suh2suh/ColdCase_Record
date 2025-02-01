using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhaseChecker))]
public class PhaseController : MonoBehaviour
{
	// TODO: 추후 Place 별로 Phase Control 다르게 해야함 -> Place: Player

	// TODO: PlaceChecker 하나 만들어서, 걸맞는 place를 자동으로 불러올 것
	//             집에 따라 달라지기 때문에, 각 집에 collider 설치 후 place 판정(enter 시 변경)
	[SerializeField] PlaceInfo placeInfo;
	[SerializeField] LoadNewScene newSceneLoader;
	// 추후, notebookInfo도 place에 따라 자동으로 불러오게 하기

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


			// TODO:정리
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

	/// <summary>  각 Npc가 사건 경위 영상을 모두 시청했을 때 -> Phase 업데이트  </summary>
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
	
	/// <summary>  모든 증거가 수집되고, 확인되었다면 -> Phase 업데이트  </summary>
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

	// 페이즈 영상 시작 -> 영상 끝나면 페이즈 업데이트
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
		/// ONLY BEFORE BUILD: Test 용으로 Z일때 초기화
		if (placeInfo.Phase == nullChar || placeInfo.Phase == 'Z') TryUpdatePhase();
	}

}
