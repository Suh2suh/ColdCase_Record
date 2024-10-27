using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioMixerManager", menuName = "ScriptableObjects/Setting/AudioMixerManager", order = 1)]
public class AudioMixerManager : ScriptableObject
{
	// 이름 AudioManager로 바꾸기

	public AudioMixer audioMixer;

	public enum audioType
	{
		Master, Music,
		Effect, Dialogue
	}

	Dictionary<audioType, string> audioTypeMixerKeyPair = new()
	{
		{ audioType.Master, "Master Volume" },
		{ audioType.Music, "Music Volume" },
		{ audioType.Effect, "Effect Volume" },
		{ audioType.Dialogue, "Dialogue Volume" }
	};
	public Dictionary<audioType, string> AudioTypeMixerKeyPair { get => audioTypeMixerKeyPair; }


	public string GetAudioFloatName(audioType audioFloatName)
	{
		return audioTypeMixerKeyPair[audioFloatName];
	}

	public void SetVolume(audioType audioFloatName, float value)
	{
		if( audioTypeMixerKeyPair.ContainsKey(audioFloatName) &&
			audioMixer.GetFloat(audioTypeMixerKeyPair[audioFloatName], out var prevValue))
		{
			audioMixer.SetFloat(audioTypeMixerKeyPair[audioFloatName], value);

			//Debug.Log(audioFloat[audioFloatName] + " Set: " + prevValue + " -> " + value);
		}
		else
		{
			Debug.Log("[ERROR] Volume Setting Error: NO KEY");
		}
	}
	public void SetVolume(audioType audioFloatName, string value)
	{
		float audioVolume = GetRealVolume(value);
		SetVolume(audioFloatName, audioVolume);
	}


	int minDB = -60;
	int maxDB = 5;
	//int volumeStep = 10;
	//int volumeStep = 1;

	public float GetRealVolume(string strValue)
	{
		if(float.TryParse(strValue, out float fValue))
		{
			//float audioVolume = minimumVolume + fValue * volumeStep;
			//float audioVolume = minimumVolume + fValue;
			float sliderParse = fValue / 100;
			if (sliderParse <= 0) sliderParse = 0.001f;

			//float audioVolume = (Mathf.Log10(volumeParse) * 20);
			float audioVolume = (sliderParse == 0.001f ? minDB : (Mathf.Log10(sliderParse) * 20) + 5);
			


			return audioVolume;
		}
		else
		{
			Debug.Log("[ERROR] AudioMixerManager: Volume Convert Error");

			return -1;
		}
	}

	/*
	public static AudioMixerManager LoadAudioMixerManager()
	{
		return ScriptableObject.CreateInstance<AudioMixerManager>();
	}*/

}
