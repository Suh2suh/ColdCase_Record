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

        // TODO: ���Ŀ��� �÷��̽����� ��� ��Ʈ���� �ٸ��� �� ������ ����
        // -> place�� hold notebook prefab ������ parent�� �ΰ�, npcInteractor���� �ش� ������ �ҷ����� �ϸ� ��
        //CreateNotebook();
        //DeActivateNotebook();
    }

	void Update()
    {
        CheckDialogueEvent();   // E Ű �ߺ� ������ Update�� �־�� ��
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


    /// <summary> Notebook Hold �Ϸ� ����, OnNpcDialogueStart Action ���� </summary>
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

    // ��Ʈ�� ��� �� -> ���� �� ù ��ȭ���, ���� ��ȭ �� ���� -> ��Ʈ�� ��� -> ��ȭ ����

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




    // TODO: ī�޶� �ػ�, ������ ���� Z�Ÿ� ����
    Vector3 GetNotebookSpawnPos()
    { return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 4, -Screen.height / 2, notebookDistance)); }

    Vector3 GetNotebookHoldPos()
    { return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 4, Screen.height - Screen.height / 8, notebookDistance)); }




    Quaternion GetCamLookQuaternion()
    { return Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up); }



    #endregion

}
