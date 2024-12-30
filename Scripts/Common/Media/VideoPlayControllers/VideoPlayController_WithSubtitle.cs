using UnityEngine;


[RequireComponent(typeof(NarrationSubtitlePlayer_Time))]
public class VideoPlayController_WithSubtitle : VideoPlayController
{
	[Space(20)]
	[Header("<Video/Subtitle Data>")]
	[SerializeField] NarrationSubtitlePlayer_Time narrationSubtitlePlayer;
	[SerializeField] UDictionary<string, VideoSubtitleData> videoSubtitleDataPairs;

	private VideoSubtitleData playingVideoSubtitleDataPair;
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
