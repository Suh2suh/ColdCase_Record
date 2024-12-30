using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[CreateAssetMenu(fileName = "AudioMixerManager", menuName = "ScriptableObjects/Setting/AudioMixerManager", order = 1)]
public class AudioMixerManager : ScriptableObject
{
	public enum audioType
	{
		Master, Music,
		Effect, Dialogue
	}


	public AudioMixer audioMixer;
	Dictionary<audioType, string> audioTypeMixerKeyPair = new()
	{
		{ audioType.Master, "Master Volume" },
		{ audioType.Music, "Music Volume" },
		{ audioType.Effect, "Effect Volume" },
		{ audioType.Dialogue, "Dialogue Volume" }
	};
	public Dictionary<audioType, string> AudioTypeMixerKeyPair 
	{ get => audioTypeMixerKeyPair; }


	public void SetVolume(audioType audioFloatName, float value)
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
	public void SetVolume(audioType audioFloatName, string value)
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


	public string GetAudioFloatName(audioType audioFloatName)
	{
		return audioTypeMixerKeyPair[audioFloatName];
	}


}
