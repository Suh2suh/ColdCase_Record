using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public abstract class VideoPlayController : MonoBehaviour
{
	#region Variables

	[Header("<Common>")]
	[SerializeField] protected GameObject videoMonitorPanel;
	[SerializeField] protected VideoPlayer videoPlayer;
	protected RawImage videoMonitor;

	[Space(10)]
	[SerializeField] protected float delayTimeAfterVideo;



	[Space(20)]
	[Header("<Fade Option>")]
	[SerializeField] protected bool isFadeWithPlaying = false;
	// 아래는 videoPlayer_WithFade 상속 하나 만들어서 거기로 분리하기
	[SerializeField] float fadeDuration = 1.5f;

	[Space(10)]
	[SerializeField] AudioSource bgmAudioSource;
	[SerializeField, Range(0, 1)] float goalVolumeInFadeIn = 0.5f;

	Image panelImageComponent;
	Color origninalPanelImageColor;
	Color transparentPanelImageColor;

	float originalMusicVolume;



	[Space(20)]
	[Header("<Skip Option>")]
	[SerializeField] bool isFullSkippableWithKey = false;
	[SerializeField] KeyCode fullSkipKey;

	[Space(10)]
	[SerializeField] bool isParitalSkippableWithClick = false;
	[Tooltip("Video is skipped to each time section if you click with mouse.\n" +
				 "No need to assign in \"VideoPlayerController_WithSubtitle\" since it's assigned automatically related to subtitle time")]
	[SerializeField] protected List<float> partialSkipTimePoints;

	[Space(10)]
	[SerializeField] GameObject skipHintTextPrefab;
	GameObject skipHintTextObj;


	bool isCurrentVideoControllerPlaying;
	public System.Action OnVideoPlayFinished;


	#endregion

	#region Unity Methods
	protected virtual void Awake()
	{
		videoMonitor = videoMonitorPanel.GetComponentInChildren<RawImage>(true);
		isCurrentVideoControllerPlaying = false;

		if (isFadeWithPlaying)
		{
			panelImageComponent = videoMonitorPanel.GetComponent<Image>();
			origninalPanelImageColor = panelImageComponent.color;
			transparentPanelImageColor = new Color(origninalPanelImageColor.r, origninalPanelImageColor.g, origninalPanelImageColor.b, 0);
		}
	}

	protected virtual void Start()
	{
		ActivateMonitorPanel(false);

		if (isFadeWithPlaying)
			panelImageComponent.color = transparentPanelImageColor;
	}

	protected virtual void Update()
	{

		if ( ! isCurrentVideoControllerPlaying || ( ! isFullSkippableWithKey && ! isParitalSkippableWithClick) )   return;


		if (skipHintTextObj == null)
		{
			if (Input.anyKeyDown)   CreateSkipHintText();
			return;
		}


		if (isFullSkippableWithKey)
		{
			if (Input.GetKeyDown(fullSkipKey))   videoPlayer.Stop();
		}

		if (isParitalSkippableWithClick)
		{
			if (Input.GetMouseButtonDown(0))
			{
				foreach (var timePoint in partialSkipTimePoints)
				if (timePoint > videoPlayer.time)
				{
					videoPlayer.time = timePoint;
					return;
				}

				videoPlayer.Stop();   // return되지 않았다면 -> skip할 구역은 다 지나친 것: 바로 비디오 끝내기
			}
		}

	}

	#endregion


	public abstract void PlayVideoWithKey(string videoClipKey);
	/*
	 * { videoClip = List.Find(videoClipKey);...
	 *   PrepareVideoClip(videoClip);
	 * }
	 */


	/// <summary>  
	/// Only Play VideoClip 
	/// </summary>
	public void PlayVideoClip(VideoClip videoClip)
	{
		videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

		videoPlayer.clip = videoClip;
		videoPlayer.Prepare();
	}

	protected virtual void CallBackOnVideoPrepareCompleted() { }

	protected virtual void CallBackOnVideoPlay() { }



	#region [Action]: Video Play Processing

	private void OnVideoPrepareCompleted(VideoPlayer source)
	{
		isCurrentVideoControllerPlaying = true;

		if ( ! isFadeWithPlaying)
		{
			ActivateMonitor(true);
			ActivateMonitorPanel(true);
			PlayVideoPlayer(source);
		}
		else
		{
			if(bgmAudioSource)   originalMusicVolume = bgmAudioSource.volume;
			ActivateMonitor(false);
			StartCoroutine(PlayVideoPlayerWithFade(source));
		}

		CallBackOnVideoPrepareCompleted();

		if (isParitalSkippableWithClick == true)
			isParitalSkippableWithClick = (partialSkipTimePoints != null && partialSkipTimePoints.Count > 0);
	}



	/// <summary>  
	/// videoPlayer.Play();  
	/// </summary>
	private void PlayVideoPlayer(VideoPlayer videoPlayer)
	{
		videoPlayer.Play();
		CallBackOnVideoPlay();

		if (this != null && this.gameObject.activeSelf)
			StartCoroutine(AlertOnVideoEnd());
	}

	private IEnumerator PlayVideoPlayerWithFade(VideoPlayer videoPlayer)
	{
		ActivateMonitorPanel(true);
		yield return StartCoroutine(FadeInOut(isIn: true));

		ActivateMonitor(true);

		PlayVideoPlayer(videoPlayer);
	}



	private IEnumerator AlertOnVideoEnd()
	{
		while(videoPlayer.isPlaying)
			yield return null;

		yield return new WaitForSecondsRealtime(delayTimeAfterVideo);

		yield return CallBackOnVideoComplete();

		if (OnVideoPlayFinished != null) OnVideoPlayFinished();
	}

	private IEnumerator CallBackOnVideoComplete() 
	{
		if ( ! isFadeWithPlaying)
		{
			ActivateMonitor(false);
			PostprocessVideoPlayer();
		}
		else
		{
			yield return StartCoroutine(PostprocessVideoPlayerWithFade());
			PostprocessVideoPlayer();
		}
	}



	private void PostprocessVideoPlayer()
	{
		isCurrentVideoControllerPlaying = false;

		videoPlayer.prepareCompleted -= OnVideoPrepareCompleted;

		ActivateMonitorPanel(false);
		videoPlayer.clip = null;
	}

	private IEnumerator PostprocessVideoPlayerWithFade()
	{
		ActivateMonitor(false);
		yield return StartCoroutine(FadeInOut(isIn: false));
	}



	private IEnumerator FadeInOut(bool isIn)
	{
		float time = 0f;
		float lerpT = time / fadeDuration;

		Color initialColor = (isIn ? transparentPanelImageColor : origninalPanelImageColor);
		Color goalColor = (isIn ? origninalPanelImageColor : transparentPanelImageColor);

		float initialVolume = (isIn ? originalMusicVolume : originalMusicVolume * goalVolumeInFadeIn);
		float goalVolume = (isIn ? originalMusicVolume * goalVolumeInFadeIn : originalMusicVolume);


		while (time < fadeDuration)
		{
			panelImageComponent.color = Color.Lerp(initialColor, goalColor, lerpT);
			if(bgmAudioSource)   bgmAudioSource.volume = Mathf.Lerp(initialVolume, goalVolume, lerpT);

			//time += Time.deltaTime;
			time += Time.unscaledDeltaTime;
			lerpT = time / fadeDuration;

			yield return null;
		}

		panelImageComponent.color = goalColor;
		if(bgmAudioSource)   bgmAudioSource.volume = goalVolume;

	}


	#endregion


	#region Utility

	private void ActivateMonitorPanel(bool activeStatus)  {  ActivationController.ActivateObj(videoMonitorPanel, activeStatus);  }

	private void ActivateMonitor(bool activeStatus) 
	{  
		if (videoMonitor.gameObject.activeSelf != activeStatus)
		{
			videoMonitor.gameObject.SetActive(activeStatus);
			if (activeStatus == false)   DestroySkiptHintText();
		}
	}
	
	private void CreateSkipHintText() { if (videoMonitor && ! skipHintTextObj)   skipHintTextObj = Instantiate(skipHintTextPrefab, videoMonitor.transform, false); }
	private void DestroySkiptHintText() { if (skipHintTextObj)   Destroy(skipHintTextObj); }


	#endregion


}
