using System.Collections;
using UnityEngine;


[System.Serializable]
public class ObtainableObject
{
	#region Setting Varialbes
	[SerializeField] private Evidence evidenceType;
	[SerializeField] private ShaderGraphEffectType effectType;
	[SerializeField] private ShaderGraphEffectDirection phaseDirection;   // 일단 지금은 Phase에만 적용됨

	#endregion

	public Evidence EvidenceType { get => evidenceType; }
	public ShaderGraphEffectType EffectType { get => effectType; }
	public ShaderGraphEffectDirection PhaseDirection { get => phaseDirection; }


	public void DeActivateIfIsObtained(GameObject obtainableObject)
	{
		if (evidenceType.IsObtained && obtainableObject.activeSelf)
			obtainableObject.SetActive(false);
	}


}