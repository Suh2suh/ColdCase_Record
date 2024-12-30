using System.Collections;
using UnityEngine;


[System.Serializable]
public class ObtainableObject
{
	[SerializeField] private Evidence evidenceType;
	[SerializeField] private ShaderGraphEffectType effectType;
	[SerializeField] private ShaderGraphEffectDirection phaseDirection;   // 일단 지금은 Phase에만 적용됨


	#region Getter
	[HideInInspector] public Evidence EvidenceType { get => evidenceType; }
	[HideInInspector] public ShaderGraphEffectType EffectType { get => effectType; }
	[HideInInspector] public ShaderGraphEffectDirection PhaseDirection { get => phaseDirection; }

	/*
	{
		get
		{
			if (EffectType != ShaderGraphEffectType.Phase)
				Debug.Log("[ERROR] InteractiveEntityInfo: Cannot Apply Obtain Effect, " +
									"you might forgot to set InteractiveEntityInfo's object type to Obtaiable Obj or to set effect type to Phase");

			return phaseDirection;
		}
	}
	*/
	#endregion


	public void DeActivateIfIsObtained(GameObject obtainableObject)
	{
		if (evidenceType.IsObtained && obtainableObject.activeSelf)
			obtainableObject.SetActive(false);
	}


}