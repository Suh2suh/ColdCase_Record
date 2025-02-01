using System.Collections;
using UnityEngine;


[RequireComponent(typeof(BlenderShapeController))]
public class NpcEyeShapeController : MonoBehaviour
{
	[SerializeField] private Vector2 eyeBlinkingTermRange = new Vector2(5f, 10f);
	[SerializeField, Range(0.1f, 0.4f)] private float eyeOpeningTime = 0.18f;
	[SerializeField, Range(0.1f, 0.4f)] private float eyeClosingTime = 0.14f;

	private BlenderShapeController npcBlenderShapeController;


	private void Start()
	{
		npcBlenderShapeController = this.GetComponent<BlenderShapeController>();

		StartCoroutine(BlinkEye());
	}


	private WaitForSecondsRealtime waitForSecondsRealtime = new(0f);
	private float eyeBlinkingTerm;
	private IEnumerator BlinkEye()
	{
		//Debug.Log("Blink");

		yield return CloseEye();
		yield return OpenEye();

		eyeBlinkingTerm = Random.Range(eyeBlinkingTermRange[0], eyeBlinkingTermRange[1]);
		waitForSecondsRealtime.waitTime = eyeBlinkingTerm;
		yield return waitForSecondsRealtime;

		StartCoroutine(BlinkEye());
	}


	private IEnumerator OpenEye()
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

	private IEnumerator CloseEye()
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
