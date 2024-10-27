using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
public class NPCDialogueManager : MonoBehaviour
{

    
    [HideInInspector, SerializeField] DialogueInfo dialogueInfo;

    [SerializeField] GameObject decisionPointPanelPrefab;
    [SerializeField] GameObject decisionPointBtnPrefab;

    [SerializeField] GameObject npcDialoguePanelPrefab;
    [SerializeField] GameObject npcDialogueTextPrefab;

    [SerializeField] Transform playerCanvas;
    [SerializeField] Transform scribbleAudioSourcesParent;

    GameObject npcDialoguePanel;
    GameObject decisionPointPanel;

    SkinnedMeshRenderer faceRemoteController;
    SkinnedMeshRenderer teethRemoveController;


    UDictionary<string, CommunicationData> currentNpcDialogue;
    List<AudioSource> scribbleAudioSources = new();


    const string selectParse = "-select-";
    int sentenceIndex;   // A-"1", A-b-"2", B-a-"1" 


    bool isTryToSkipDialogue = false;
    bool isTextTyping = false;

    DialogueSheet currentTalkingNpc;
    Transform currentTalkingNpcTransform;

    List<string> dialogueLog;



	private void Awake()
	{
        foreach (var scribbleAudioSource in scribbleAudioSourcesParent.transform.GetComponentsInChildren<AudioSource>(true))
            scribbleAudioSources.Add(scribbleAudioSource);

    }

	private void Start()
	{
        dialogueInfo.isDialogueNeed = false;
        dialogueInfo.isNpcValid = false;
        dialogueInfo.initializeCorrectChoiceTree();

        dialogueLog = new();
    }

	void Update()
    {
        if (dialogueInfo.isDialogueNeed)
		{
            if (dialogueInfo.isNpcValid && dialogueInfo.holdingNotebook)
            {
                //입 오물
                ReadyToMoveNpcMouth();

                //다이얼로그
                currentTalkingNpc = dialogueInfo.CurrentTalkingNpc;
                currentNpcDialogue = dialogueInfo.NpcDialogueDictionary[currentTalkingNpc];
                AddViralKeys();

                sentenceIndex = 1;
                dialogueInfo.ValidateNpcDialogueData();

                CreateNPCDialoguePanel();

                if (dialogueInfo.CheckPlayerPhaseMatchWith(currentTalkingNpc))
				{
                    StartCoroutine(ShowDialogueText(GetDialogueKey(sentenceIndex)));
                }
                else
				{
                    var randomViralKey = viralKeys[Random.Range(0, viralKeys.Count)];
                    StartCoroutine(ShowDialogueText(randomViralKey, isViral: true));
                }

                dialogueInfo.isNpcValid = false;
            }


            var isSkipKeyPressed = (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
            var isDecisionEventOn = decisionPointPanel;

            if (isSkipKeyPressed && !isDecisionEventOn)
            {
                if (isTextTyping)
				{
                    isTryToSkipDialogue = true;
                }
                else
				{
                    if (dialogueInfo.CheckPlayerPhaseMatchWith(currentTalkingNpc))
                        TryShowNextDialogueText();

                    else
                        EndDialogue(isViral: true);
                }
                    
            }
        }
        else
		{
            if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Talking)
			{
                EndDialogue();
            }

		}


    }

	#region Initialization

	List<string> viralKeys = new();
    void AddViralKeys()
    {
        var checkIndex = 1;
        var viralParse = "Viral-";

        while (currentNpcDialogue.ContainsKey(viralParse + checkIndex.ToString()))
        {
            viralKeys.Add(viralParse + checkIndex.ToString());

            checkIndex++;
        }
    }




	#endregion


	void TryShowNextDialogueText()
    {
        var nextDialogueKey = GetDialogueKey(sentenceIndex + 1);

        if (currentNpcDialogue.ContainsKey(nextDialogueKey))
        {
            sentenceIndex += 1;
            StartCoroutine(ShowDialogueText(nextDialogueKey));
        }
        else
        {
            EndDialogue();
        }


    }

    IEnumerator ShowDialogueText(string dialogueKey, bool isViral = false)
    {
        StartCoroutine(AnimateMouse());

        if (!isTextTyping)
        {
            isTextTyping = true;

            var dialogueText = GetDialogueText(dialogueKey);
            var dialogueTextObj = Instantiate(npcDialogueTextPrefab, npcDialoguePanel.transform, false);
            var dialogueTextTMPro = dialogueTextObj.GetComponent<TextMeshProUGUI>();

            string parserText = "";
            int parseIndex = 0;

            while (parserText.Length < dialogueText.Length)
            {
                parserText += dialogueText[parseIndex];
                parseIndex++;

                dialogueTextTMPro.text = parserText;

                PlayScribbleAudioRandomly();

                if (isTryToSkipDialogue)
                {
                    isTryToSkipDialogue = false;
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }
            dialogueTextTMPro.text = dialogueText;

            StopAllScribbleAudios();
            CloseMouth(perfectClose: false);

            isTextTyping = false;

            // 다음 키 탐색

            if (!isViral)
            {
                CheckNextDialogueEvent();
                dialogueLog.Add(dialogueText);
            }

        }
    }




    /// <summary>  return "N-select-select-select-sentenceIndex"  </summary>
    string GetDialogueText(string dialogueKey)
    {
        return currentNpcDialogue[dialogueKey].dialogueByLanguage[dialogueInfo.language];
    }
    string GetDialogueKey(int sentenceIndex)
    {
        var dialogueKey = GetLatestDialogueSection() + ("-" + sentenceIndex.ToString());

        return dialogueKey;
    }
    /// <summary>  return "N-select-select-select"  </summary>
    string GetLatestDialogueSection()
    {
        return dialogueInfo.GetLatestDialoguePhaseOf(currentTalkingNpc) + dialogueInfo.GetLatestPlayerChoiceRecordOf(currentTalkingNpc);
    }



	#region Npc Mouse Animation

	float minOpenWeight = 10f;
    //float maxOpenWeight = 60f;

    IEnumerator AnimateMouse()
	{
        var currentOpenWeight = faceRemoteController.GetBlendShapeWeight(0);

        float lerpGoal = 0f;
        // 지금 입이 열린 상태라면,
        if (currentOpenWeight > minOpenWeight)
		{
            var closeWeight = Random.Range(0, 11);
            lerpGoal = closeWeight;
        }
        else
		{
            var maxOpenWeight = minOpenWeight + Random.Range(2, 11) * 5;
            lerpGoal = maxOpenWeight;
        }

        var duration = 1 / (Mathf.Abs(currentOpenWeight - lerpGoal) / 4);
        var t = duration;

        do
        {
            var weight = Mathf.Lerp(currentOpenWeight, lerpGoal, t);
            OpenMouth(weight);

            t += duration;

            yield return null;
        } while (t < 1);

        if(isTextTyping)
            StartCoroutine(AnimateMouse());
	}

    void OpenMouth(float weight)
    {
        faceRemoteController.SetBlendShapeWeight(0, weight); // mouse open
        teethRemoveController.SetBlendShapeWeight(0, weight); // teeth
    }
    void CloseMouth(bool perfectClose)
    {
        if (perfectClose)
        {
            faceRemoteController.SetBlendShapeWeight(0, 0); // mouse open
            teethRemoveController.SetBlendShapeWeight(0, 0); // teeth
        }
    }

    void ReadyToMoveNpcMouth()
    {
        currentTalkingNpcTransform = dialogueInfo.CurrentTalkingNpcTransform;

        var playerFace = currentTalkingNpcTransform.GetChild(0).Find("Body");
        var playerTeeth = currentTalkingNpcTransform.GetChild(0).Find("Teeth_Lower");


        faceRemoteController = playerFace.GetComponent<SkinnedMeshRenderer>();
        teethRemoveController = playerTeeth.GetComponent<SkinnedMeshRenderer>();
    }

    
// 단어별 -> 너무 프레임 끊겨보여서 사용 X

int spokenWord = 0;
int speakingTerm = 0;
float prevWeight = -1;

void PlayMouseAnimationRandomly()
{
    spokenWord++;

    if (spokenWord > speakingTerm)
    {

        float weight;

        do
        {
            weight = 30 + Random.Range(1, 11) * 3;
        } while (weight == prevWeight);
        prevWeight = weight;
        OpenMouth(weight);

        spokenWord = 0;
        speakingTerm = Random.Range(1, 3);
    }
}
    
    #endregion


    #region Scribble Audio Playing

    // Play Dialogue Sounds: Scribble on notebook

    int writtenWord = 0;
    int scribbleTerm = 0;
    int prevScribbleAudioIndex = -1;
    void PlayScribbleAudioRandomly()
    {
        writtenWord++;

        if (writtenWord > scribbleTerm)
        {
            StopAllScribbleAudios();

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, scribbleAudioSources.Count);
            } while (randomIndex == prevScribbleAudioIndex);
            prevScribbleAudioIndex = randomIndex;

            var randomAudioSource = scribbleAudioSources[randomIndex];

            randomAudioSource.PlayOneShot(randomAudioSource.clip);

            writtenWord = 0;
            scribbleTerm = Random.Range(2, 4);
        }
    }
    void StopAllScribbleAudios()
    {
        foreach (var scribbleAudioSource in scribbleAudioSources)
        {
            if (scribbleAudioSource.isPlaying) scribbleAudioSource.Stop();
        }

        writtenWord = 0;
    }


    #endregion



    #region Player Decision Event


    void CheckDecisionPointEvent()
    {
        var selectKey = GetLatestDialogueSection() + selectParse;
        char decisionPoint = 'a';

        if (currentNpcDialogue.ContainsKey(selectKey + decisionPoint))
        {
            var selectKeysInRow = new List<string>();

            do
            {
                selectKeysInRow.Add(selectKey + decisionPoint);

                decisionPoint++;

            } while (currentNpcDialogue.ContainsKey(selectKey + decisionPoint));

            CreateDecisionPanel();
            CreateDecisionPoints(selectKeysInRow);

            //return true;
        }

        //return false;
    }

    void DecisionPointOnClick(string decisionPoint)
    {
        DestroyDecisionPanel();

        dialogueInfo.UpdatePlayerChoiceRecord(currentTalkingNpc, decisionPoint);

        // 로그 투입
        dialogueLog.Add("select -> " + decisionPoint);

        sentenceIndex = 1;
        StartCoroutine(ShowDialogueText(GetDialogueKey(sentenceIndex)));

        //Debug.Log(decisionPoint + " is selected");
    }


    #endregion



    void CheckNextDialogueEvent()
	{
        var isNextDialogueKeyValid = ValidateNextDialogueKey();

        if(!isNextDialogueKeyValid)
        {
            // 선택 이벤트인지 체크
            //var isDecisionPointHeld = CheckDecisionPointEvent();
            CheckDecisionPointEvent();
        }
    }
    bool ValidateNextDialogueKey()
    {
        var nextDialogueKey = GetDialogueKey(sentenceIndex + 1);

        return currentNpcDialogue.ContainsKey(nextDialogueKey);
    }







    void EndDialogue(bool isViral = false)
    {
        if(!isViral)
		{
            var correctChoiceForThisPhase = dialogueInfo.CorrectChoiceTree[currentTalkingNpc][dialogueInfo.GetLatestDialoguePhaseOf(currentTalkingNpc)];
            var playerChoiceForThisPhase = dialogueInfo.GetLatestPlayerChoiceRecordOf(currentTalkingNpc);

            Debug.Log(correctChoiceForThisPhase);
            Debug.Log(playerChoiceForThisPhase);

            if (correctChoiceForThisPhase == playerChoiceForThisPhase)
            {
                dialogueInfo.UpdatePlayerChoiceRecord(currentTalkingNpc);
            }
            else
            {
                dialogueInfo.ResetPlayerChoiceRecord(currentTalkingNpc);
            }
        }

        StopAllCoroutines();
        StopAllScribbleAudios();
        CloseMouth(perfectClose: true);
        isTextTyping = false;

        dialogueInfo.isDialogueNeed = false;
        DestroyNPCDialoguePanel();
        DestroyDecisionPanel();
        PlayerStatusManager.SetInterStatus(InteractionStatus.None);
    }    
   



    #region Create/Destroy Prefabs (Panel, Button...)


    void CreateNPCDialoguePanel()
	{
        if (!npcDialoguePanel)
            npcDialoguePanel = Instantiate(npcDialoguePanelPrefab, dialogueInfo.holdingNotebook.GetComponentInChildren<Canvas>().transform, false);
            //npcDialoguePanel = Instantiate(npcDialoguePanelPrefab, playerCanvas, false);

    }
    void DestroyNPCDialoguePanel()
	{
        if (npcDialoguePanel)
            Destroy(npcDialoguePanel);
    }

    void CreateDecisionPanel()
	{
        if(!decisionPointPanel)
            decisionPointPanel = Instantiate(decisionPointPanelPrefab, playerCanvas, false);
	}
    void DestroyDecisionPanel()
	{
        if (decisionPointPanel)
            Destroy(decisionPointPanel);

    }
    void CreateDecisionPoints(List<string> selectEventKeys)
    {
        foreach (var selectEventKey in selectEventKeys)
        {
            var decisionPointBtn = Instantiate(decisionPointBtnPrefab, decisionPointPanel.transform, false);
            var decisionPointAlphabet = selectEventKey[selectEventKey.Length - 1].ToString();

            decisionPointBtn.name = decisionPointAlphabet;
            decisionPointBtn.GetComponentInChildren<TextMeshProUGUI>().text = currentNpcDialogue[selectEventKey].dialogueByLanguage[dialogueInfo.language];

            //Debug.Log(decisionPointBtn.GetComponent<Button>());
            decisionPointBtn.GetComponent<Button>().onClick.AddListener(() => DecisionPointOnClick(decisionPointBtn.name));

            //activeDecisionPointBtns.Add(decisionPointBtn);
        }
    }


    #endregion


    #region ONLY BEFORE BUILD : for inspector button
    public void ResetNpcDialogueData()
	{
        dialogueInfo.ResetNpcDialogueData();

    }
    public void UpdatePlayerPhase()
	{
        dialogueInfo.UpdatePlayerDialoguePhase();
    }


	#endregion
    
}
    */
