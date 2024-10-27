using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


// 나중에 상속으로 더 자르기 -> basicVideoController이 다 상속 (UDictionary나 List 빼고 ㅇㅇ) 얘넨 걍 상속하는 애들이 넣기
[RequireComponent(typeof(NarrationSubtitlePlayer_Time))] //시간많을때 하나 공유했을 때 에러나는 원인 보기
public class VideoPlayController_WithSubtitle : VideoPlayController
{
	[Space(20)]
	[Header("<Video/Subtitle Data>")]
	[SerializeField] NarrationSubtitlePlayer_Time narrationSubtitlePlayer;
	[SerializeField] UDictionary<string, VideoSubtitleData> videoSubtitleDataPairs;

	VideoSubtitleData playingVideoSubtitleDataPair;

	public System.Action OnVideoPlayStart;



	public override void PlayVideoWithKey(string videoClipKey)
	{
		foreach (var videoSubtitleDataPair in videoSubtitleDataPairs)
		{
			if (videoSubtitleDataPair.Key == videoClipKey)
			{
				playingVideoSubtitleDataPair = videoSubtitleDataPair.Value;

				break;
			}
		}
		if (playingVideoSubtitleDataPair == null) return;

		PlayVideoClip(playingVideoSubtitleDataPair.VideoClip);
	}

	protected override void CallBackOnVideoPrepareCompleted()
	{
		partialSkipTimePoints = new();

		foreach (var subtitleData in playingVideoSubtitleDataPair.SubtitleData)
			partialSkipTimePoints.Add(subtitleData.startTime);
	}

	protected override void CallBackOnVideoPlay()
	{
		narrationSubtitlePlayer.StartVideoSubtitle(videoPlayer, playingVideoSubtitleDataPair.SubtitleData);

		if (OnVideoPlayStart != null) OnVideoPlayStart();
	}


}
