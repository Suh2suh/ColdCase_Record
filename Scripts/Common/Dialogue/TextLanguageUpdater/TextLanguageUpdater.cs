using System.Collections;
using TMPro;
using UnityEngine;


public abstract class TextLanguageUpdater : MonoBehaviour
{
	[SerializeField] DialogueInfo dialogueInfo;


	#region Unity Methods

	private void Awake()
	{
		SettingManager.OnLanguageChanged += OnLanguageChanged;
	}

	private void Start()
	{
		ApplyTextWithKeys();
	}

	private void OnDestroy()
	{
		SettingManager.OnLanguageChanged -= OnLanguageChanged;
	}


	#endregion


	void OnLanguageChanged()
	{
		Debug.Log(dialogueInfo.language);
		ApplyTextWithKeys();
	}



	protected abstract void ApplyTextWithKeys();

	protected void ApplyTranslatedText(TextMeshProUGUI textObj, string sheetName, string keyCode)
	{
		//Debug.Log(dialogueInfo.GetTranslatedText(sheetName, keyCode));
		textObj.text = dialogueInfo.GetTranslatedText(sheetName, keyCode);
	}



}