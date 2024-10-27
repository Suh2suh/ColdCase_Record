using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "VideoSubtitleData", menuName = "ScriptableObjects/Objects/VideoSubtitleData", order = 2)]
public class VideoSubtitleData : ScriptableObject
{
	[SerializeField, Space(15)] VideoClip videoClip;
	[SerializeField] List<NarrativeSubtitleData_Time> subtitleData;

	#region Getters

	public VideoClip VideoClip { get => videoClip; }
	public List<NarrativeSubtitleData_Time> SubtitleData { get => subtitleData; }

	#endregion
}