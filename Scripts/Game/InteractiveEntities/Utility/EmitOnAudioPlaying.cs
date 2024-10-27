using System.Collections;
using UnityEngine;

public class EmitOnAudioPlaying : MonoBehaviour
{
	[SerializeField] AudioSource targetAudioSource;
	Material targetMaterial;
	bool isEmitOn;


	private void Awake()
	{
		targetMaterial = GetComponent<MeshRenderer>().material;
		isEmitOn = false;
	}


	private void Update()
	{
		if(targetAudioSource.isPlaying)
		{
			if( ! isEmitOn) TurnEmissionOn();
		}
		else
		{
			if (isEmitOn) TurnEmissionOff();
		}
	}



	void ChangeEmissionColor(Color emissionColor, float intensity)
	{
		targetMaterial.SetColor("_EmissiveColor", emissionColor * intensity);
	}

	// 아래 부분은 emit 올리기
	void TurnEmissionOn()
	{
		ChangeEmissionColor(Color.white, 1f);

		isEmitOn = true;
	}
	public void TurnEmissionOff()
	{
		ChangeEmissionColor(Color.black, 0f);

		isEmitOn = false;
	}


}