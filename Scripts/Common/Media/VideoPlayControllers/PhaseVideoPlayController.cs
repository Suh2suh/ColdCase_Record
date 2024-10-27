using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class PhaseVideoPlayController : VideoPlayController_WithAudio
{
	[Space(20)]
	[Header("<Phase Video Subttle>")]
	[SerializeField] DialogueInfo dialogueInfo;
	[SerializeField] TextMeshProUGUI phaseVideoTitle;

	
	protected override void CallBackOnVideoPrepareCompleted()
	{
		SetPhaseTitle("Phase_" + currentPlayingVideoClipKey);
	}

	private void SetPhaseTitle(string phaseTitleKey)
	{
		phaseVideoTitle.text = dialogueInfo.CommonDialogueDictionary["Narration"][phaseTitleKey][dialogueInfo.language];
	}


}