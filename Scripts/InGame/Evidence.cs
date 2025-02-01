using UnityEngine;


[CreateAssetMenu(fileName = "Evidence", menuName = "ScriptableObjects/Objects/Evidence", order = 1)]
public class Evidence : ScriptableObject
{
	[Header("is Obtained or Photo taken?")]
	[SerializeField] bool isObtained;

	[Header("is Checked by player on the detective desk?")]
	[SerializeField] bool isChecked;


	public static System.Action<Evidence> OnEvidenceObtained;


	#region Getters / Setters

	public bool IsObtained { get => isObtained; set => isObtained = value; }
	public void SetIsObtained(bool obtainStatus)
	{
		if (isObtained != obtainStatus)
		{
			isObtained = obtainStatus;

			if (OnEvidenceObtained != null) OnEvidenceObtained(this);


#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}


	public bool IsChecked { get => isChecked; set => isChecked = value; }
	public void SetIsChecked(bool checkedStatus)
	{
		if (isChecked != checkedStatus)
		{
			isChecked = checkedStatus;


#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}


	#endregion

}
