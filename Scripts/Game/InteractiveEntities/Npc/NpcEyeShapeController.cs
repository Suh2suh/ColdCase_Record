using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BlenderShapeController))]
public class NpcEyeShapeController : MonoBehaviour
{

	BlenderShapeController npcBlenderShapeController;
	[SerializeField] Vector2 eyeBlinkingTermRange = new Vector2(5f, 10f);
	[SerializeField, Range(0.1f, 0.4f)] float eyeOpeningTime = 0.18f;
	[SerializeField, Range(0.1f, 0.4f)] float eyeClosingTime = 0.14f;



	private void Start()
	{
		npcBlenderShapeController = this.GetComponent<BlenderShapeController>();

		StartCoroutine(BlinkEye());
	}


	WaitForSecondsRealtime waitForSecondsRealtime = new(0f);
	float eyeBlinkingTerm;
	IEnumerator BlinkEye()
	{
		//Debug.Log("Blink");

		yield return CloseEye();
		yield return OpenEye();

		eyeBlinkingTerm = Random.Range(eyeBlinkingTermRange[0], eyeBlinkingTermRange[1]);
		waitForSecondsRealtime.waitTime = eyeBlinkingTerm;
		yield return waitForSecondsRealtime;

		StartCoroutine(BlinkEye());
	}


	
	IEnumerator OpenEye()
	{
		float time = 0;

		while(time < eyeOpeningTime)
		{
			npcBlenderShapeController.SetBlenderShapeValue("Eye_Close", (1 - time / eyeOpeningTime) * 100);

			time += Time.deltaTime;
			yield return null;
		}
		npcBlenderShapeController.SetBlenderShapeValue("Eye_Close", 0f);

	}


	IEnumerator CloseEye()
	{
		float time = 0;

		while (time < eyeClosingTime)
		{
			npcBlenderShapeController.SetBlenderShapeValue("Eye_Close", (time / eyeClosingTime) * 100);

			time += Time.deltaTime;
			yield return null;
		}
		npcBlenderShapeController.SetBlenderShapeValue("Eye_Close", 100f);

	}



}
