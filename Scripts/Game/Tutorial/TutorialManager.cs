using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// Attatchable to Any object
public class TutorialManager : MonoBehaviour
{

	[SerializeField] TutorialInfo tutorialInfo;
	[SerializeField] DialogueInfo dialogueInfo;
	[SerializeField] PlayerInfo playerInfo;
	[SerializeField] GameObject narrationPanel;
	[SerializeField] GameObject policeLineForBlock;

	[SerializeField, Space(15)] List<Transform> tutorialSequenceForTools;

	[SerializeField, Space(15)] NarrationData_Clip StartNarrationData;
	[SerializeField] NarrationData_Clip EndNarrationData;


	int currentToolNum;
	Dictionary<Transform, DetectiveTool> detectiveToolDic;
	public Dictionary<Transform, DetectiveTool> DetectiveToolDic { get => detectiveToolDic; }

	[SerializeField] AudioSource audioSource;
	TextMeshProUGUI narrationText;

	Transform lastTool;

	static bool isToolHandleEnd;
	public static bool IsToolHandleEnd
	{
		get => isToolHandleEnd;
	}


	public static System.Action<DetectiveTool> OnTutorialToolUpdated;

	#region Getters / Setters

	#endregion


	#region Unity Methods

	private void Awake()
	{
		TutorialInfo.OnDetectiveToolTutorialed += OnDetectiveToolTutorialed;
		PlayerStatusManager.OnInteractionStatusUpdated += OnInteractionStatusUpdated;
		PlaceInfo.OnPhaseUpdated += OnPhaseUpdated;

		tutorialInfo.currentTutorialTool = null;
		detectiveToolDic = new();
		foreach (var toolTransform in tutorialSequenceForTools)
		{
			var detectiveTool = toolTransform.GetComponent<DetectiveToolInfo>().ToolType;
			detectiveToolDic.Add(toolTransform, detectiveTool);
			if( ! tutorialInfo.IsTutorialEnd) detectiveTool.isTutorialed = false;
		}

		narrationText = narrationPanel.GetComponentInChildren<TextMeshProUGUI>();
		ActivateNarrationPanel(false);
		
	}

	private void Start()
	{
		if (PhaseChecker.GetCurrentPhase() == 'A')
		{
			if (!tutorialInfo.IsTutorialEnd)
			{
				PreProcessTutorial();
				PlayNarration(StartNarrationData);
			}
			else
			{
				PostProcessTutorial();
			}
		}
		else
		if (PhaseChecker.GetCurrentPhase() > 'A' && PhaseChecker.GetCurrentPhase() != 'Z') //TODO: 추후에는 NULL이 아니고, a초과일때로
		{
			PostProcessTutorial();
		}
			
	}


	void OnPhaseUpdated(PlaceInfo updatedPlace, char phase)
	{


		if(phase == 'A')
		{
			if ( ! tutorialInfo.IsTutorialEnd)
			{
				PreProcessTutorial();
				PlayNarration(StartNarrationData);
			}
		}
	}




	private void OnDisable()
	{
		TutorialInfo.OnDetectiveToolTutorialed -= OnDetectiveToolTutorialed;
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
		PlaceInfo.OnPhaseUpdated -= OnPhaseUpdated;
	}

	/*
	private void OnDestroy()
	{
		TutorialInfo.OnDetectiveToolTutorialed -= OnDetectiveToolTutorialed;
		PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
	}
	*/

	#endregion



	//void OnInteractionStatusUpdated(InteractionStatus prevStatus, InteractionStatus currentStatus)
	void OnInteractionStatusUpdated()
	{
		if (!tutorialInfo.IsTutorialEnd && tutorialInfo.currentTutorialTool == null &&
			PlayerStatusManager.GetPrevInterStatus() == InteractionStatus.None &&
			PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Investigating)
			StartTutorial();
	}



	void OnDetectiveToolTutorialed(Transform tutorialedDetectiveTool)
	{
		detectiveToolDic[tutorialedDetectiveTool].SetIsTutorialed(true);
		if (detectiveToolDic[tutorialedDetectiveTool].name == "Polaroid")
			StartCoroutine(DestroyObjectWithDissolve(tutorialedDetectiveTool));

		// -> if (audioSource.재생중) audioClip 종료

		//Debug.Log(tutorialedDetectiveTool + " / " + lastTool);

		if (tutorialedDetectiveTool == lastTool)
		{
			isToolHandleEnd = true;
			PlayNarration(EndNarrationData);
			//PostProcessTutorial();

			return;
		}


		currentToolNum++;
		var newTool = tutorialSequenceForTools[currentToolNum];


		UpdateTutorialTool(newTool);

		//detectiveToolDic[tutorialedDetectiveTool]
	}


	void UpdateTutorialTool(Transform newTool)
	{
		tutorialInfo.currentTutorialTool = newTool;

		//Debug.Log("new tool: " + tutorialInfo.currentTutorialTool.name);

		var mouseHoverEmitter = tutorialInfo.currentTutorialTool.GetComponent<EmitOnMouseHover>();
		mouseHoverEmitter.SetBlinkCondition(EmitOnMouseHover.BlinkCondition.Always);

		// 오디오 재생
		PlayNarration(detectiveToolDic[newTool].tutorialNarrationData);

		OnTutorialToolUpdated(detectiveToolDic[newTool]);

		//Debug.Log("update -> " + detectiveToolDic[newTool].name);
	}



	void PreProcessTutorial()
	{
		ActivatePoliceLine();
		
		currentToolNum = 0;
		lastTool = tutorialSequenceForTools[tutorialSequenceForTools.Count - 1];
		isToolHandleEnd = false;

		narrationText.alpha = 0;
		ActivateNarrationPanel(true);
	}


	void StartTutorial()
	{
		var firstTool = tutorialSequenceForTools[0];

		UpdateTutorialTool(firstTool);
	}

	IEnumerator EndTutorial()
	{
		yield return StartCoroutine(DestroyObjectWithDissolve(policeLineForBlock.transform));

		PostProcessTutorial();
	}

	void PostProcessTutorial(bool isFirstClear = false)
	{
		DeActivatePoliceLine();

		tutorialInfo.currentTutorialTool = null;

		ActivateNarrationPanel(false);

		this.gameObject.SetActive(false);
	}



	IEnumerator narrationCoroutine;
	void PlayNarration(NarrationData_Clip narrationData)
	{
		audioSource.Stop();
		if(narrationCoroutine != null) StopCoroutine(narrationCoroutine);

		audioSource.PlayOneShot(narrationData.narrationAudioClip);

		narrationCoroutine = ShowSubtitlePerNarration(narrationData);
		StartCoroutine(narrationCoroutine);
	}



	float subtitleTextFadeStage = 10f;

	IEnumerator ShowSubtitlePerNarration(NarrationData_Clip narrationData)
	{
		if (narrationData.narrationAudioClip == EndNarrationData.narrationAudioClip)
			tutorialInfo.SetTutorialStatus(true);

		// ALERT:   여러개 재생할거면 그냥 반복문 돌리면 됨 지금은 걍 하나만 할 거라서
		var subtitleData = narrationData.subtitleDatas[0];
		var subtitleText = dialogueInfo.CommonDialogueDictionary["Narration"][subtitleData.key][dialogueInfo.language];
		var audioTime = narrationData.narrationAudioClip.length * (1 / audioSource.pitch);

		narrationText.alpha = 0;
		narrationText.text = subtitleText;

		float time = 0;


		while (narrationText.alpha < 1)
		{
			narrationText.alpha += (1 / subtitleTextFadeStage);

			yield return null;
		}
		narrationText.alpha = 1;


		while(time < audioTime)
		{
			time += Time.deltaTime;

			yield return null;
		}

		narrationText.alpha = 0;

		if (narrationData.narrationAudioClip == EndNarrationData.narrationAudioClip)
			StartCoroutine(EndTutorial());
	}


	[SerializeField] MaterialEffectManager materialEffectManager;
	[SerializeField] float destroyingDuration = 3.0f;

	IEnumerator DestroyObjectWithDissolve(Transform destoryingObj)
	{
		yield return StartCoroutine(materialEffectManager.ApplyMaterialEffect(destoryingObj, ShaderGraphEffectType.Dissolve, ShaderGraphEffectDirection.None, destroyingDuration));

		destoryingObj.gameObject.SetActive(false);
	}



	void ActivatePoliceLine() { if (!policeLineForBlock.activeSelf) policeLineForBlock.SetActive(true); }

	void DeActivatePoliceLine() { if (policeLineForBlock.activeSelf) policeLineForBlock.SetActive(false); }

	void ActivateNarrationPanel(bool activeStatus) { if (narrationPanel.activeSelf != activeStatus) narrationPanel.SetActive(activeStatus); }


}
