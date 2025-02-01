using UnityEngine;
using TMPro;
using ColdCase.Dialogue.Book.Object;


public class TextLanguageUpdater_MixedSheet : TextLanguageUpdater
{

	[Space(10)]
	[Header("Add More Text In Need")]
	[SerializeField] protected UDictionary<TextMeshProUGUI, BookSentence> applyingKeyPerTargetText;
	[HideInInspector] public UDictionary<TextMeshProUGUI, BookSentence> ApplyingKeyPerTargetText { get => applyingKeyPerTargetText; }


	protected override void ApplyTextWithKeys()
	{
		foreach (var keyTextPair in applyingKeyPerTargetText)
		{
			TextMeshProUGUI textObj = keyTextPair.Key;
			var sentenceInfo = keyTextPair.Value;

			ApplyTranslatedText(textObj, sentenceInfo.sheetName, sentenceInfo.keyCode);
		}
	}


}