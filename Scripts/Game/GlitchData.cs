using System.Collections;
using UnityEngine;

public class GlitchData : MonoBehaviour
{
	[SerializeField] GameObject glitchObject;
	[SerializeField] string glitchNarrationKey;

	#region Getters

	public GameObject GlitchObject { get => glitchObject; }
	public string GlitchNarrationKey { get => glitchNarrationKey; }


	#endregion


}