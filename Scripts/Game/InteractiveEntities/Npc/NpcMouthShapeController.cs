using System.Collections;
using UnityEngine;


public class NpcMouthShapeController
{
	// "a", "e", "i", "o", "u"
	/*
	 
	 a: teeth 100
	 e: teeth 100
	  i: teeth 0
	 o: teeth 100
	 u: teeth 100
	 
	 */

	BlenderShapeController targetBlenderShapeController;

	public readonly string teethKey = "Mouth_Open";
	float mouseDuration = 0.5f;
	Pronounce prevPronounce = Pronounce.None;

	bool isMouthOpen = false;
	bool isTeethOpen = false;

	public Pronounce PrevPronounce { get => prevPronounce; }
	public bool IsMouthOpen { get => isMouthOpen; }

	public NpcMouthShapeController(BlenderShapeController blenderShapeController)
	{
		targetBlenderShapeController = blenderShapeController;
	}


	// open & close at the same time
	public IEnumerator OpenMouthSmoothly(Pronounce pronounce, float intensity, float duration)
	{
		//Debug.Log("closing: " + prevPronounce + " / opening: " + pronounce);

		float time = 0;
		float lerpT = time / mouseDuration;


		float prevOriginalValue = 0f;
		if (prevPronounce != Pronounce.None) 
			prevOriginalValue = targetBlenderShapeController.GetBlenderShapeValue(prevPronounce.ToString());
		float originalValue = targetBlenderShapeController.GetBlenderShapeValue(pronounce.ToString());
		float teethOriginalValue = targetBlenderShapeController.GetBlenderShapeValue(teethKey);


		isMouthOpen = (intensity > 0);

		while (time < duration)
		{
			if (prevPronounce != Pronounce.None)
				OpenMouth(prevPronounce, Mathf.Lerp(prevOriginalValue, 0, lerpT));
			OpenMouth(pronounce, Mathf.Lerp(originalValue, intensity, lerpT));



			if (pronounce == Pronounce.i)
				OpenTeeth(Mathf.Lerp(teethOriginalValue, 0, lerpT));
			else
				OpenTeeth(Mathf.Lerp(teethOriginalValue, intensity, lerpT));



			time += Time.deltaTime;
			lerpT = time / mouseDuration;

			//Debug.Log(time +  " < " + duration);

			yield return null;
		}

		if (prevPronounce != Pronounce.None)
			OpenMouth(prevPronounce, 0);
		OpenMouth(pronounce, intensity);


		if (intensity != 0) prevPronounce = pronounce;
		else					   prevPronounce = Pronounce.None;
	}


	public void OpenMouth(Pronounce pronounce, float intensity)
	{
		targetBlenderShapeController.SetBlenderShapeValue(pronounce.ToString(), intensity);

		//Debug.Log(intensity);
	}

	public void OpenTeeth(float openIntensity)
	{
		targetBlenderShapeController.SetBlenderShapeValue(teethKey, openIntensity);
	}


}
