using UnityEngine;


class AudioSourceController
{
	AudioSource targetAudioSource;
	bool isAudioClipPlaying;


	// communicating npc가 시작할 때에만 하면 되기 때문에, 상관 X
	public AudioSourceController(AudioSource targetAudioSource)
	{
		this.targetAudioSource = targetAudioSource;
	}

	public void PlayAudioClip(AudioClip audioClip)
	{
		targetAudioSource.clip = audioClip;

		targetAudioSource.Play();
	}

	public void StopAudioClip()
	{
		if(targetAudioSource.isPlaying)  targetAudioSource.Stop();
	}

	public bool IsAudioSourcePlaying()
	{
		return targetAudioSource.isPlaying;
	}

}