using System.Collections;
using TMPro;
using UnityEngine;


public class NarrationSubtitlePlayer : MonoBehaviour
{
	[Space(15)]
	[SerializeField] protected GameObject narrativeDialoguePanel;
	[SerializeField] protected DialogueInfo dialogueInfo;
	[SerializeField] protected float subtitleTextFadeStage = 10f;

	protected TextMeshProUGUI displayText;
	protected Coroutine currentNarrationCoroutine;


	#region Unity Methods

	private void Start()
	{
		displayText = narrativeDialoguePanel.transform.GetComponentInChildren<TextMeshProUGUI>();
	}


	#endregion


	/// <summary>
	/// show subtitle constantly
	/// </summary>
	public void ShowSubtitleConstantly(string sheetName, string subtitleKey)
	{
		if (currentNarrationCoroutine != null) StopCoroutine(currentNarrationCoroutine);

		currentNarrationCoroutine = StartCoroutine(ShowSubtitle(sheetName, subtitleKey));
	}


	public void ShowSubtitleBriefly(string sheetName, string subtitleKey, float durationTime)
	{
		if (currentNarrationCoroutine != null) StopCoroutine(currentNarrationCoroutine);

		if (durationTime > 0)
			 currentNarrationCoroutine = StartCoroutine(ShowSubtitleBriefly(sheetName, subtitleKey, new WaitForSecondsRealtime(durationTime)));
		else
			currentNarrationCoroutine = StartCoroutine(ShowSubtitleBriefly(sheetName, subtitleKey, null));
	}

	private IEnumerator ShowSubtitleBriefly(string sheetName, string subtitleKey, IEnumerator callBackCoroutine)
	{
		ActivateNarrationPanel(true);


		yield return (ShowSubtitle(sheetName, subtitleKey));

		yield return callBackCoroutine;


		ActivateNarrationPanel(false);

	}


	protected IEnumerator ShowSubtitle(string sheetName, string subtitleKey)
	{
		ActivateNarrationPanel(true);


		string subtitleText = dialogueInfo.GetTranslatedText(sheetName, subtitleKey);

		displayText.alpha = 0;
		displayText.text = subtitleText;


		while (displayText.alpha < 1)
		{
			displayText.alpha += (1 / subtitleTextFadeStage);

			yield return null;
		}
		
		displayText.alpha = 1;
	}


	public void ActivateNarrationPanel(bool activeStatus)
	{
		Debug.Log(narrativeDialoguePanel);
		if (narrativeDialoguePanel.activeSelf != activeStatus)  narrativeDialoguePanel.SetActive(activeStatus);
	}


}
