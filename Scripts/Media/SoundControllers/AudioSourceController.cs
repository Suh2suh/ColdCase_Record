using UnityEngine;


class AudioSourceController
{
	private AudioSource targetAudioSource;


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