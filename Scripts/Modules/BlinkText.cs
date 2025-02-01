using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
	TextMeshProUGUI targetText;
	bool isShown;
	[SerializeField] float blinkStage = 30f;

	bool isKeepBlink = true;
	public bool IsKeepBlink 
	{
		get => isKeepBlink;
		set
		{
			if (isKeepBlink != value)
			{
				isKeepBlink = value;

				if (isKeepBlink)
					StartCoroutine(MakeThisBlink());
				else
				{
					StopAllCoroutines();
					targetText.color = GetColorWithAlpha(1);
					isShown = true;
				}
			}
		}
	}


	private void OnEnable()
	{
		if(isKeepBlink)
		{
			targetText = this.transform.GetComponent<TextMeshProUGUI>();
			targetText.color = GetColorWithAlpha(0);
			isShown = false;

			StartCoroutine(MakeThisBlink());
		}
	}


	IEnumerator MakeThisBlink()
	{
		//Debug.Log(this.gameObject.name);

		if(isShown)
		{
			var alpha = 1.0f;
			while(targetText.color.a > 0)
			{
				targetText.color = GetColorWithAlpha(alpha);

				alpha -= (1 / blinkStage);
				yield return null;
			}
			targetText.color = GetColorWithAlpha(0);
			isShown = false;

		}
		else
		{
			var alpha = 0.0f;
			while (targetText.color.a < 1)
			{
				targetText.color = GetColorWithAlpha(alpha);

				alpha += (1 / blinkStage);
				yield return null;
			}
			targetText.color = GetColorWithAlpha(1);
			isShown = true;

		}

		StartCoroutine(MakeThisBlink());
	}


	Color GetColorWithAlpha(float a)
	{
		return new Color(targetText.color.r, targetText.color.g, targetText.color.b, a);
	}

}
