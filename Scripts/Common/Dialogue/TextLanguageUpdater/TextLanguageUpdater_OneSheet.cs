using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextLanguageUpdater_OneSheet : TextLanguageUpdater
{

	[Space(10)]
	[SerializeField] string sheetName;
	[Header("Add More Text In Need")]
	[SerializeField] List<Container> KeyTextPairContainers;


	protected override void ApplyTextWithKeys()
	{
		//Debug.Log("hi");
		foreach (var container in KeyTextPairContainers)
		{
			foreach(var keyTextPair in container.ApplyingKeyPerTargetText)
			{
				var textObj = keyTextPair.Key;
				var keyCode = keyTextPair.Value;

				ApplyTranslatedText(textObj, sheetName, keyCode);
			}
		}
	}


	[System.Serializable]
	struct Container
	{
		[SerializeField] UDictionary<TextMeshProUGUI, string> applyingKeyPerTargetText;
		[HideInInspector] public UDictionary<TextMeshProUGUI, string> ApplyingKeyPerTargetText { get => applyingKeyPerTargetText; }
	}


}