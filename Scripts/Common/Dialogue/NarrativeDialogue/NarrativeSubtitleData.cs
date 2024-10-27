using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NarrativeSubtitleData_Duration
{
	/// <summary>  �ڸ� Sheet Key  </summary>
	public string key;

	/// <summary> �ش� �ð����� �ڸ� ǥ�� </summary>
	public float duration;
}



[System.Serializable]
public struct NarrativeSubtitleData_Time
{
	/// <summary>  �ڸ� Sheet Key  </summary>
	public string key;

	/// <summary> �ش� �ð��� �ڸ� ǥ�� </summary>
	public float startTime;
	public float endTime;
}
