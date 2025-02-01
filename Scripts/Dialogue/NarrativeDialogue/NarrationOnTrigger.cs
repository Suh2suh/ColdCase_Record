using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationOnTrigger : MonoBehaviour
{
    public NarrationData[] narrationData;
	[SerializeField] DialogueInfo dialogueInfo;

	private void OnTriggerEnter(Collider other)
	{
		dialogueInfo.isNarationNeed = true;
		dialogueInfo.SetNewNarrationData(narrationData);
	}

}