using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// csv reading
using TMPro;

public class NarrativeDialogueManager : MonoBehaviour
{
	[SerializeField] GameObject narrativeDialoguePanel;
	[SerializeField] GameObject skipHintText;
	[SerializeField] DialogueInfo dialogueInfo;
	TextMeshProUGUI displayText;

	[SerializeField] float subtitleTextFadeStage = 10f;

	NarrationData[] NarrationDatas;
	NarrativeSubtitleData_Duration[] SubtitleDatasPerNarration;

	List<AudioSource> PlayingAudioSource = new();

	/// <summary>  한 사운드가 재생될 때마다 true로, 한 사운드 끝날 시 false  </summary>
	bool isNarationOn = false;
	/// <summary>  나레이션 시작 단계 컨트롤, 없을 시 데이터 미준비 상태로 나레이션 시작하는 예외 처리 불가  </summary>
	bool isNarationDataReady = false;
	/// <summary>  스킵 가능 여부 컨트롤, 플레이어가 나레이션 플레이 중 아무 키나 눌렀을 때 True  </summary>
	bool isSkipKeyReady = false;
	/// <summary>  EndDialogue로 나레이션이 끝났을 때에만. dialogueInfo로 옮겨도 될 것 같기도  </summary>
	//bool isNarrationOver = false;

	int narrationIndex = 0;
	int subtitleIndex = 0;

	int allNarrationNum = -1;
	int subtitleNumPerNarration = -1;


	private void Start()
	{
		displayText = narrativeDialoguePanel.transform.GetComponentInChildren<TextMeshProUGUI>();

		ActivateNarrationPanel(false);
	}

	private void Update()
	{
		// TODO: Action으로 바꾸기
		if(dialogueInfo.isNarationNeed)
		{
			if(dialogueInfo.isNarrationValid)
			{
				dialogueInfo.isNarrationValid = false;

				ReadyToStartNarration();
			}


			if(isNarationDataReady && !isNarationOn)
			{
				//Debug.Log(narrationIndex + "번째 나레이션 데이터 재생 시도...");
				if (narrationIndex < allNarrationNum)
				{
					isNarationOn = true;

					PlayAudioPerNarration();
					PlaySubtitlePerNarration();
				}
				else
				{
					EndNarration();
					ActivateNarrationPanel(false);
				}
			}


			if(isNarationOn)
			{
				if (!isSkipKeyReady && Input.anyKeyDown)
				{
					ActivateSkipHintText();

					isSkipKeyReady = true;
				}

				if (isSkipKeyReady)
				{
					if (Input.GetKeyDown(KeyCode.Space))
					{
						StopAllCoroutines();
						StopAllPlayingAudios();

						EndNarration();

						ActivateNarrationPanel(false);
						DeActivateSkiptHintText();

						isSkipKeyReady = false;
					}
					else if (Input.GetMouseButtonDown(0))
					{
						StopAllCoroutines();

						narrationIndex++;
						isNarationOn = false;
					}
				}
			}



		}

	}

	protected virtual void EndNarration()
	{
		dialogueInfo.isNarationNeed = false;
		isNarationOn = false;
		isNarationDataReady = false;
	}



	void ReadyToStartNarration()
	{
		ActivateNarrationPanel(true);

		NarrationDatas = dialogueInfo.CurrentNarrationData;
		narrationIndex = 0;
		allNarrationNum = NarrationDatas.Length;

		// Debug.Log("출력할 나레이션 사운드: " + allNarrationNum + "개");
		isNarationDataReady = true;
	}






	#region Subtitle Play


	void PlaySubtitlePerNarration()
	{
		SubtitleDatasPerNarration = NarrationDatas[narrationIndex].subtitleDatas;
		subtitleNumPerNarration = SubtitleDatasPerNarration.Length;
		subtitleIndex = 0;


		if (subtitleNumPerNarration > 0)
		{
			var subtitleData = SubtitleDatasPerNarration[subtitleIndex];
			StartCoroutine(ShowSubtitlesPerNarration(subtitleData));
		}
		else
		{
			narrationIndex++;
			isNarationOn = false;
		}
	}


	IEnumerator ShowSubtitlesPerNarration(NarrativeSubtitleData_Duration subtitleData)
	{
		string subtitleText;
		if (subtitleData.key != "" || subtitleData.key != null)
			subtitleText = dialogueInfo.CommonDialogueDictionary["Narration"][subtitleData.key][dialogueInfo.language];
		else
			subtitleText = "";

		displayText.alpha = 0;
		displayText.text = subtitleText;


		while (displayText.alpha < 1)
		{
			displayText.alpha += (1 / subtitleTextFadeStage);

			yield return null;
		}
		displayText.alpha = 1;


		yield return new WaitForSecondsRealtime(subtitleData.duration);
		TryPlayNextSubtitle();


		yield return null;
	}
	void TryPlayNextSubtitle()
	{
		subtitleIndex++;

		if (subtitleIndex < subtitleNumPerNarration)
		{
			//Debug.Log("다음 자막 재생");

			StartCoroutine(ShowSubtitlesPerNarration(SubtitleDatasPerNarration[subtitleIndex]));
		}
		else
		{
			//Debug.Log("재생할 자막 없음");

			isNarationOn = false;
			narrationIndex++;
		}
	}


	#endregion


	#region Audio Play

	void PlayAudioPerNarration()
	{
		StopAllPlayingAudios();

		var narrationAudioSource = NarrationDatas[narrationIndex].narrationAudio;
		if (narrationAudioSource)
		{
			PlayingAudioSource.Add(narrationAudioSource);
			narrationAudioSource.PlayOneShot(narrationAudioSource.clip);
		}
	}

	void StopAllPlayingAudios()
	{
		if (PlayingAudioSource.Count != 0)
		{
			foreach (var playingAudio in PlayingAudioSource)
				if (playingAudio.isPlaying)
					playingAudio.Stop();

			PlayingAudioSource.Clear();
		}
	}


	#endregion



	#region Activate/DeActivate GameObjects

	void ActivateSkipHintText()
	{
		if (!skipHintText.activeSelf) skipHintText.SetActive(true);
	}
	void DeActivateSkiptHintText()
	{
		if (skipHintText.activeSelf) skipHintText.SetActive(false);
	}


	// 나레이션패널 오프되는 문제 해겷ㄱ
	void ActivateNarrationPanel(bool activeStatus) { if (!narrativeDialoguePanel.activeSelf != activeStatus) narrativeDialoguePanel.SetActive(activeStatus); }


	#endregion


}

