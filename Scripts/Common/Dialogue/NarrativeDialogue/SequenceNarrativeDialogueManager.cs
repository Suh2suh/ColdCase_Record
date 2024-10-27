using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNarrativeDialogueManager : NarrativeDialogueManager
{
	[SerializeField] SceneInfo sceneInfo;
	[SerializeField] LoadNewScene newSceneLoader;

	protected override void EndNarration()
	{
		base.EndNarration();

		if(sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Sequence)
		{
			newSceneLoader.LoadANewScene("Stage_1");
		}
	}
}
