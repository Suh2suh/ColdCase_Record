using System;
using System.Collections;
using UnityEngine;

public class NpcInteractor : MonoBehaviour
{

    [SerializeField] DialogueInfo dialogueInfo;

    [SerializeField, Space(15)] GameObject notebookPrefab;
    [SerializeField] Camera topRenderCamera;

    float notebookDistance = 0.3f;
    float notebookHoldDuration = 0.5f;

    bool isMovingCoroutineOn = false;


    #region Unity Methods

    private void Awake()
	{
        DialogueInfo.OnNpcDialogueEnd += DropNotebook;
        NotebookInfo.OnNotebookHoldReady += HoldNotebook;
    }

	private void Start()
	{
        isMovingCoroutineOn = false;

        // TODO: 추후에는 플레이스별로 드는 노트북을 다르게 할 여지가 있음
        // -> place별 hold notebook prefab 지정한 parent를 두고, npcInteractor에서 해당 정보를 불러오게 하면 됨
        //CreateNotebook();
        //DeActivateNotebook();
    }

	void Update()
    {
        CheckDialogueEvent();   // E 키 중복 때문에 Update에 넣어야 함
    }

	private void OnDestroy()
	{
        DialogueInfo.OnNpcDialogueEnd -= DropNotebook;
        NotebookInfo.OnNotebookHoldReady -= HoldNotebook;
    }



	#endregion



	/// <summary>  Hold Notebook & Start Dialogue  </summary>
	void CheckDialogueEvent()
    {
        if(HotKeyChecker.isKeyPressed[HotKey.Talk] && ObjectSorter.CHPointingObj.ObjType == ObjectType.NPC)
		{
            if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
            {
                if (!isMovingCoroutineOn)   PreProcessNpcDialogue();

                return;
            }
        }

        if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingNpc)
		{
            var isSkipKeyPressed = HotKeyChecker.isKeyPressed[HotKey.Talk] || HotKeyChecker.isKeyPressed[HotKey.Escape];
            if (isSkipKeyPressed)
			{
                if (!isMovingCoroutineOn)   DialogueInfo.OnNpcDialogueEnd();
            }
		}

    }


    /// <summary> Notebook Hold 완료 이후, OnNpcDialogueStart Action 실행 </summary>
    void PreProcessNpcDialogue()
    {
        PlayerStatusManager.SetInterStatus(InteractionStatus.TalkingNpc);

        var communicatingNpc = ObjectSorter.CHPointingObj.ObjTransform;
        var npcPlaceInfo = communicatingNpc.GetComponent<InteractiveEntityInfo>().NpcInfo.NpcPlace;
        if (npcPlaceInfo == null)
		{
            Debug.Log("Npc Type in InteractiveEntityInfo is not set! Unable to Initialize");
            return;
        }
        else
		{
            CreateNotebookOnPlace(npcPlaceInfo);
            DialogueInfo.OnNpcClicked(communicatingNpc);
        }

        //Debug.Log("PREPROCESS");
    }




    // OnNotebookLog Initialized.
    void HoldNotebook()
    {
        dialogueInfo.holdingNotebook.gameObject.SetActive(true);

        StartCoroutine(HoldNotebookAndPostprocess());
    }

    void DropNotebook()
    {
        StartCoroutine(DropNotebookAndPostprocess());
    }



    IEnumerator HoldNotebookAndPostprocess()
	{
        var holdingNotebook = dialogueInfo.holdingNotebook;
        yield return StartCoroutine(LerpNotebookPos(holdingNotebook, GetNotebookHoldPos, notebookHoldDuration));

        PostprocessNotebookHold(holdingNotebook);
    }
    IEnumerator DropNotebookAndPostprocess()
    {
        var holdingNotebook = dialogueInfo.holdingNotebook;
        yield return StartCoroutine(LerpNotebookPos(holdingNotebook, GetNotebookSpawnPos, notebookHoldDuration));

        PostprocessNotebookDrop(holdingNotebook);
    }



    IEnumerator LerpNotebookPos(Transform notebook, Func<Vector3> getPosAction, float duration)
    {
        isMovingCoroutineOn = true;
        bool isLerpForHold = (getPosAction == GetNotebookHoldPos);


        float time = 0;
        Vector3 objPos = notebook.position;
        Quaternion objRot = notebook.rotation;


        while (time < duration)
        {
            if (isLerpForHold)
                notebook.SetPositionAndRotation(Vector3.Lerp(objPos, getPosAction(), time / duration), 
                                                                       Quaternion.Lerp(objRot, GetCamLookQuaternion(), time / duration));
            else
                notebook.position = Vector3.Lerp(objPos, getPosAction(), time / duration);

            time += Time.deltaTime;

            yield return null;
        }
        notebook.position = getPosAction();

        isMovingCoroutineOn = false;

    }



    void PostprocessNotebookHold(Transform notebook)
	{
        notebook.rotation = GetCamLookQuaternion();
        var npc = ObjectSorter.CHPointingObj.ObjTransform;

        DialogueInfo.OnNpcDialogueStart();
    }

    void PostprocessNotebookDrop(Transform notebook)
    {
        DestroyNotebook();

        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.TalkingNpc)
            PlayerStatusManager.SetInterStatus(InteractionStatus.None);
    }



    #region Manage Notebook

    // 노트북 들기 전 -> 게임 중 첫 대화라면, 이전 대화 다 쓰기 -> 노트북 들기 -> 대화 시작

    void CreateNotebookOnPlace(PlaceInfo npcPlaceInfo)
    {
        var notebookRot = GetCamLookQuaternion();
        var notebook = Instantiate(notebookPrefab, GetNotebookSpawnPos(), notebookRot, this.transform);
        //notebook.transform.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        notebook.transform.GetComponentInChildren<Canvas>().worldCamera = topRenderCamera;

        dialogueInfo.holdingNotebook = notebook.transform;
        NotebookInfo.OnNotebookCreated(notebook.transform, npcPlaceInfo);
    }
    void DestroyNotebook()
	{
        Destroy(dialogueInfo.holdingNotebook.gameObject);
	}




    // TODO: 카메라 해상도, 비율에 따라서 Z거리 조절
    Vector3 GetNotebookSpawnPos()
    { return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 4, -Screen.height / 2, notebookDistance)); }

    Vector3 GetNotebookHoldPos()
    { return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 4, Screen.height - Screen.height / 8, notebookDistance)); }




    Quaternion GetCamLookQuaternion()
    { return Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up); }



    #endregion

}
