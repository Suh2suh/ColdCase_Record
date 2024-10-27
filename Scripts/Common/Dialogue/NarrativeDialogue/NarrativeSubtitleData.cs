using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NarrativeSubtitleData_Duration
{
	/// <summary>  자막 Sheet Key  </summary>
	public string key;

	/// <summary> 해당 시간동안 자막 표출 </summary>
	public float duration;
}



[System.Serializable]
public struct NarrativeSubtitleData_Time
{
	/// <summary>  자막 Sheet Key  </summary>
	public string key;

	/// <summary> 해당 시간에 자막 표출 </summary>
	public float startTime;
	public float endTime;
}
