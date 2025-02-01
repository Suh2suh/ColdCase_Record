using System.Collections;
using UnityEngine;


/// <summary>
/// Role: open npc's mouth with "A/E/I/O/U"
/// </summary>
public class NpcMouthShapeController
{
	public readonly string teethKey = "Mouth_Open";

	#region Private Variables
	private BlenderShapeController targetBlenderShapeController;

	private float mouseDuration = 0.5f;
	private Pronounce prevPronounce = Pronounce.None;

	private bool isMouthOpen = false;

	#endregion

	#region Properties
	public Pronounce PrevPronounce { get => prevPronounce; }
	public bool IsMouthOpen { get => isMouthOpen; }

	#endregion


	public NpcMouthShapeController(BlenderShapeController blenderShapeController)
	{
		targetBlenderShapeController = blenderShapeController;
	}


	public IEnumerator OpenMouthSmoothly(Pronounce pronounce, float intensity, float duration)
	{
		float time = 0;
		float lerpT = time / mouseDuration;

		float prevPronounceShapeValue = (prevPronounce == Pronounce.None ? 0 : targetBlenderShapeController.GetBlenderShapeValue(prevPronounce.ToString()));
		float currentPronounceShapeValue = targetBlenderShapeController.GetBlenderShapeValue(pronounce.ToString());
		float currentTeethShapeValue = targetBlenderShapeController.GetBlenderShapeValue(teethKey);

		isMouthOpen = (intensity > 0);

		while (time < duration)
		{
			if (prevPronounceShapeValue != 0)
			{
				OpenMouth(prevPronounce, Mathf.Lerp(prevPronounceShapeValue, 0, lerpT));
			}

			OpenMouth(pronounce, Mathf.Lerp(currentPronounceShapeValue, intensity, lerpT));

			float goalTeethShapeValue = (pronounce == Pronounce.i ? 0 : intensity);
			OpenTeeth(Mathf.Lerp(currentTeethShapeValue, goalTeethShapeValue, lerpT));

			time += Time.deltaTime;
			lerpT = time / mouseDuration;

			yield return null;
		}

		if (prevPronounce != Pronounce.None)
			OpenMouth(prevPronounce, 0);
		OpenMouth(pronounce, intensity);


		if (intensity != 0)  prevPronounce = pronounce;
		else			     prevPronounce = Pronounce.None;
	}


	public void OpenMouth(Pronounce pronounce, float intensity)
	{
		targetBlenderShapeController.SetBlenderShapeValue(pronounce.ToString(), intensity);
	}

	public void OpenTeeth(float openIntensity)
	{
		targetBlenderShapeController.SetBlenderShapeValue(teethKey, openIntensity);
	}


}
