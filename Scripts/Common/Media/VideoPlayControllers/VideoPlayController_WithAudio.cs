using System.Collections;
using UnityEngine;
using UnityEngine.Video;


[RequireComponent(typeof(AudioSource))]
public class VideoPlayController_WithAudio : VideoPlayController
{
	[Space(20)]
	[Header("<Video Data>")]
	[SerializeField] protected UDictionary<string, AudioPerVideoClip> videoNAudioKeyPair;

	AudioSourceController audioSourceController;
	protected string currentPlayingVideoClipKey;


	#region Unity Methods

	protected override void Awake()
	{
		base.Awake();

		audioSourceController = new(transform.GetComponent<AudioSource>());
	}


	#endregion

	public override void PlayVideoWithKey(string videoClipKey)
	{
		if (!videoNAudioKeyPair.ContainsKey(videoClipKey))
		{
			Debug.LogWarning("[NO KEY]: Cannot Play VideoClip! add" + videoClipKey + " key in inspector");
			return;
		}

		PlayVideoClip(videoNAudioKeyPair[videoClipKey].videoClip);
		currentPlayingVideoClipKey = videoClipKey;
	}


	protected override void CallBackOnVideoPlay()
	{
		if (videoNAudioKeyPair[currentPlayingVideoClipKey].audioClip != null)
			audioSourceController.PlayAudioClip(videoNAudioKeyPair[currentPlayingVideoClipKey].audioClip);
	}




	[System.Serializable]
	public struct AudioPerVideoClip
	{
		public VideoClip videoClip;
		public AudioClip audioClip;
	}


}
