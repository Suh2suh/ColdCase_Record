using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[CreateAssetMenu(fileName = "AudioMixerManager", menuName = "ScriptableObjects/Setting/AudioMixerManager", order = 1)]
public class AudioMixerManager : ScriptableObject
{

	public AudioMixer audioMixer;
	Dictionary<AudioGroup, string> audioTypeMixerKeyPair = new()
	{
		{ AudioGroup.Master, "Master Volume" },
		{ AudioGroup.Music, "Music Volume" },
		{ AudioGroup.Effect, "Effect Volume" },
		{ AudioGroup.Dialogue, "Dialogue Volume" }
	};
	public Dictionary<AudioGroup, string> AudioTypeMixerKeyPair 
	{ get => audioTypeMixerKeyPair; }


	public void SetVolume(AudioGroup audioFloatName, float value)
	{
		if( audioTypeMixerKeyPair.ContainsKey(audioFloatName) &&
			audioMixer.GetFloat(audioTypeMixerKeyPair[audioFloatName], out var prevValue))
		{
			audioMixer.SetFloat(audioTypeMixerKeyPair[audioFloatName], value);
		}
		else
		{
			Debug.Log("[ERROR] Volume Setting Error: NO KEY");
		}
	}
	public void SetVolume(AudioGroup audioFloatName, string value)
	{
		float audioVolume = GetRealVolume(value);
		SetVolume(audioFloatName, audioVolume);
	}


	int minDB = -60;
	int maxDB = 5;
	public float GetRealVolume(string strValue)
	{
		if(float.TryParse(strValue, out float fValue))
		{
			float sliderParse = fValue / 100;
			if (sliderParse <= 0) sliderParse = 0.001f;

			float audioVolume = (sliderParse == 0.001f ? minDB : (Mathf.Log10(sliderParse) * 20) + 5);
			
			return audioVolume;
		}
		else
		{
			Debug.Log("[ERROR] AudioMixerManager: Volume Convert Error");

			return -1;
		}
	}


	public string GetAudioFloatName(AudioGroup audioFloatName)
	{
		return audioTypeMixerKeyPair[audioFloatName];
	}


}
