using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;


// TODO: monobehaviour굳이필요없음 나중에 고치기
public class NarrationSubtitlePlayer_Time : NarrationSubtitlePlayer
{
	List<NarrativeSubtitleData_Time> subtitleDataList;
	VideoPlayer syncVideoPlayer;


	int subtitleIndex;

	bool isSubtitleOn;


	#region Unity Methods

	private void Awake()
	{
		subtitleIndex = 0;
		subtitleDataList = new();
	}

	private void Update()
	{
		PlaySubtitleOnUpdate();
	}

	private void OnDisable()
	{
		subtitleIndex = 0;
		subtitleDataList = new();
	}

	#endregion



	void PlaySubtitleOnUpdate()
	{
		if (syncVideoPlayer == null) return;

		if (syncVideoPlayer.isPlaying && subtitleIndex < subtitleDataList.Count)
		{
			double realVideoTime = syncVideoPlayer.time / syncVideoPlayer.playbackSpeed;

			if (!isSubtitleOn)
			{
				float realSubtitleStartTime = subtitleDataList[subtitleIndex].startTime / syncVideoPlayer.playbackSpeed;
				bool isSubtitleAppearNeed = realVideoTime > realSubtitleStartTime;
				if (isSubtitleAppearNeed)
				{
					isSubtitleOn = true;

					StartCoroutine(ShowSubtitle("Narration", subtitleDataList[subtitleIndex].key));
				}
			}
			else
			{
				float realSubtitleEndTime = subtitleDataList[subtitleIndex].endTime / syncVideoPlayer.playbackSpeed;
				bool isSubtitleDisappearNeed = realVideoTime >= realSubtitleEndTime;
				if (isSubtitleDisappearNeed)
				{
					displayText.alpha = 0;
					isSubtitleOn = false;

					subtitleIndex++;
				}
			}

		}
		else
		{
			EndVideoSubtitle();
		}

	}


	// ready 후 불러오기 때문에, 굳이 clip까지 따질필요 X.
	public void StartVideoSubtitle(VideoPlayer newVideoPlayer, List<NarrativeSubtitleData_Time> newSubtitleDatas)
	{
		syncVideoPlayer = newVideoPlayer;

		isSubtitleOn = false;
		subtitleDataList = newSubtitleDatas;
		subtitleIndex = 0;

		displayText.alpha = 0;
		ActivateNarrationPanel(true);
	}

	void EndVideoSubtitle()
	{
		displayText.alpha = 0;
		isSubtitleOn = false;

		syncVideoPlayer = null;

		ActivateNarrationPanel(false);
	}


}