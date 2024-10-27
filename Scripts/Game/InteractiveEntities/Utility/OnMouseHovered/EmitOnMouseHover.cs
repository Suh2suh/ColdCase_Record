using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmitOnMouseHover : MouseHoverChecker
{
	[SerializeField, Tooltip("This should be graphic model parent")] Transform ModelParent;

	[SerializeField, Range(0.1f, 1.0f)] float emitIntensity = 1.0f;

	//[SerializeField]
	[Space(15)] public BlinkCondition blinkCondition;

	[SerializeField, Tooltip("Duration per 1 emission"), Range(0.1f, 0.9f)] float blinkDuration = 0.5f;

	public enum BlinkCondition { Never, Always, onHover };

	Color emissionColor = Color.white;

	List<Material> materials;



	#region Unity Methods

	private void Awake()
	{
		materials = new();
	}

	private void Start()
	{
		foreach(var meshRenderer in ModelParent.GetComponentsInChildren<MeshRenderer>().ToList())
		{
			//var material = meshRenderer.materials.ToList();
			var material = meshRenderer.material;
			if (!materials.Contains(material)) materials.Add(material);
		}

		if (blinkDuration < 0.1) blinkDuration = 0.1f;

		 //if(blinkCondition == BlinkCondition.Always) StartCoroutine(BlinkMaterial(isRising: true));
	}

	private void OnEnable()
	{
		if (blinkCondition == BlinkCondition.Always) StartCoroutine(BlinkMaterial(isRising: true));
	}
	private void OnDisable()
	{
		ForceStopEmitAndBlink();
	}


	#endregion



	public void SetBlinkCondition(BlinkCondition newBlinkCondition)
	{
		blinkCondition = newBlinkCondition;
		TurnEmissionOff();

		switch (newBlinkCondition)
		{
			case BlinkCondition.Always:
				StartCoroutine(BlinkMaterial(isRising: true));

				break;
			case BlinkCondition.Never:
			case BlinkCondition.onHover:
				StopAllCoroutines();

				break;
		}
	}


	/// <summary>  Call this function when this object clicked or etc...  </summary>
	public void ForceStopEmitAndBlink()
	{
		IsMouseHovering = false;

		if (blinkCondition == BlinkCondition.Always)
			StopAllCoroutines(); // 항상이면, 마우스 떼자마자 다시 반짝하므로 코루틴 자체 해제.
		//{
			   // Blinck Condition이 Hover이라면 블링크중이므로 
			//TurnEmissionOff();     // force emission stop -> OnMouseLeave하면 어차피 꺼지므로 일단 빼봄
		//}
	}



	protected override void OnMouseHover()
	{
		if (mouseHovableStatus != PlayerStatusManager.GetCurrentInterStatus()) return;


		if (blinkCondition == BlinkCondition.onHover)
		{
			StartCoroutine(BlinkMaterial(isRising: true));
			return;
		} else
		if (blinkCondition == BlinkCondition.Always)
		{
			StopAllCoroutines();
		}


			TurnEmissionOn();
	}
	protected override void OnMouseLeave()
	{
		//if (mouseHovableStatus != PlayerStatusManager.GetCurrentInterStatus()) return;

		if (blinkCondition == BlinkCondition.onHover)
		{
			StopAllCoroutines();
		} else
		if (blinkCondition == BlinkCondition.Always)
		{
			StartCoroutine(BlinkMaterial(isRising: true));
			return;
		}
		



		TurnEmissionOff();
	}


	public IEnumerator BlinkMaterial(bool isRising)
	{
		//if(this.gameObject.name.Contains("Walkie")) Debug.Log("Blink");

		float time = 0;

		float startIntensity = (isRising ? 0f : emitIntensity);
		float endIntensity  = (isRising ? emitIntensity : 0f);
		float intensity;

		while(time < blinkDuration)
		{
			intensity = Mathf.Lerp(startIntensity, endIntensity, time / blinkDuration);
			ChangeEmissionColor(emissionColor, intensity);

			time += Time.deltaTime;

			yield return null;
		}
		ChangeEmissionColor(emissionColor, endIntensity);


		if (isRising) StartCoroutine(BlinkMaterial(false));
		else StartCoroutine(BlinkMaterial(true));

		yield return null;
	}




	// 아래 부분은 emit 올리기
	void TurnEmissionOn()
	{
		ChangeEmissionColor(Color.white, emitIntensity);
	}
	public void TurnEmissionOff()
	{
		ChangeEmissionColor(Color.black, emitIntensity);
	}




	void ChangeEmissionColor(Color emissionColor, float intensity)
	{
		foreach(var material in materials)
		{
			material.SetColor("_EmissiveColor", emissionColor * intensity);
		}
	}

}