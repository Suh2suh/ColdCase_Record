using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class NarrationSubtitlePlayer_Duration : NarrationSubtitlePlayer
{
	#region Private Varaibles
	private List<NarrativeSubtitleData_Duration> subtitleDataList;
	private VideoPlayer syncVideoPlayer;

	private int subtitleIndex;

	#endregion


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


	public void PlaySubtitle(VideoPlayer newVideoPlayer, List<NarrativeSubtitleData_Duration> newSubtitleDatas)
	{
		syncVideoPlayer = newVideoPlayer;

		subtitleDataList = newSubtitleDatas;
		subtitleIndex = 0;

		StartCoroutine(ShowSubtitlesPerNarration(subtitleDataList[subtitleIndex]));
	}


	private IEnumerator ShowSubtitlesPerNarration(NarrativeSubtitleData_Duration subtitleData)
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


	private void TryPlayNextSubtitle()
	{
		subtitleIndex++;

		if (subtitleIndex < subtitleDataList.Count)
			StartCoroutine(ShowSubtitlesPerNarration(subtitleDataList[subtitleIndex]));
		else
			ActivateNarrationPanel(false);
	}


}