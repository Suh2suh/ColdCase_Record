using System.Collections.Generic;
using UnityEngine.Video;


public class NarrationSubtitlePlayer_Time : NarrationSubtitlePlayer
{
	#region Private Variables
	private List<NarrativeSubtitleData_Time> subtitleDataList;
	private VideoPlayer syncVideoPlayer;

	private int subtitleIndex;
	private bool isSubtitleOn;

	#endregion


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


	private void PlaySubtitleOnUpdate()
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


	public void StartVideoSubtitle(VideoPlayer newVideoPlayer, List<NarrativeSubtitleData_Time> newSubtitleDatas)
	{
		syncVideoPlayer = newVideoPlayer;

		isSubtitleOn = false;
		subtitleDataList = newSubtitleDatas;
		subtitleIndex = 0;

		displayText.alpha = 0;
		ActivateNarrationPanel(true);
	}

	private void EndVideoSubtitle()
	{
		displayText.alpha = 0;
		isSubtitleOn = false;

		syncVideoPlayer = null;

		ActivateNarrationPanel(false);
	}


}