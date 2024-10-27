using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;


// TODO: monobehaviour굳이필요없음 나중에 고치기
public class NarrationSubtitlePlayer_Duration : NarrationSubtitlePlayer
{
	List<NarrativeSubtitleData_Duration> subtitleDataList;
	VideoPlayer syncVideoPlayer;


	int subtitleIndex;


	#region Unity Methods

	private void Awake()
	{
		subtitleIndex = 0;
		subtitleDataList = new();
	}

	private void OnDisable()
	{
		subtitleIndex = 0;
		subtitleDataList = new();
	}


	#endregion



	// ready 후 불러오기 때문에, 굳이 clip까지 따질필요 X.
	public void PlaySubtitle(VideoPlayer newVideoPlayer, List<NarrativeSubtitleData_Duration> newSubtitleDatas)
	{
		syncVideoPlayer = newVideoPlayer;

		subtitleDataList = newSubtitleDatas;
		subtitleIndex = 0;

		StartCoroutine(ShowSubtitlesPerNarration(subtitleDataList[subtitleIndex]));
	}



	IEnumerator ShowSubtitlesPerNarration(NarrativeSubtitleData_Duration subtitleData)
	{
		yield return StartCoroutine(ShowSubtitle("Narration", subtitleData.key));


		if (syncVideoPlayer.isPlaying)
		{
			yield return new WaitForSecondsRealtime(subtitleData.duration);
			TryPlayNextSubtitle();
		}
		else
		{
			ActivateNarrationPanel(false);
		}

	}

	void TryPlayNextSubtitle()
	{
		subtitleIndex++;

		if (subtitleIndex < subtitleDataList.Count)
			StartCoroutine(ShowSubtitlesPerNarration(subtitleDataList[subtitleIndex]));
		else
			ActivateNarrationPanel(false);
	}


}