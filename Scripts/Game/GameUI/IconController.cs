using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class IconController : Singleton<IconController>
{

    [Space(15)]
    [SerializeField] HotKeyInfo hotKeyInfo;


    [Space(20)]
    [SerializeField] GameObject fIconObj;

    [SerializeField] GameObject investigationIconObj;
    [SerializeField] GameObject walkieTalkieIconObj;
    [SerializeField] GameObject npcIconObj;

    [SerializeField] GameObject photoIconObj;

	#region Getters
    [HideInInspector] public GameObject FIconObj { get => fIconObj;  }

	#endregion


	#region Unity Methods

	protected override void Awake()
    {
        base.Awake();

        ObjectSorter.OnCHPointingObjChanged += OnCHPointingObjChanged;
        PlayerStatusManager.OnInteractionStatusUpdated += OnInteractionStatusUpdated;
    }

    private void Start()
    {
        ActivateIcon(fIconObj, false);

        ActivateIcon(investigationIconObj, false);
        ActivateIcon(walkieTalkieIconObj, false);
        ActivateIcon(npcIconObj, false);

        ActivateIcon(photoIconObj, false);
    }

    private void OnDestroy()
    {
        ObjectSorter.OnCHPointingObjChanged -= OnCHPointingObjChanged;
        PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
    }

    

    private void Update()
    {
        if(GameModeManager.GetCurrentGameMode() == GameMode.Game)
		{
            if (photoIconObj.activeSelf)
                DisplayIconOnObj(photoIconObj, ObjectSorter.CHPointingObj.ObjTransform);
        }
    }


    #endregion



    public void ActivateIcon(GameObject IconObj, bool activeStatus)
    {
        if (IconObj.activeSelf != activeStatus) IconObj.SetActive(activeStatus);
    }

    public void DisplayIconOnObj(GameObject IconObj, Transform targetObj)
    {
        // Show F Icon
        if (!IconObj.activeSelf) IconObj.SetActive(true);


        // Attatch F Icon near hint Obj
        if (targetObj)
        {
            var screenPos = Camera.main.WorldToScreenPoint(targetObj.position);
            IconObj.transform.position = screenPos;
        }
    }



    void OnCHPointingObjChanged(IObjectInfo pointingObj)
    {
        HintPlayerPhotographicEvidence(pointingObj);
    }

    void OnInteractionStatusUpdated()
    {
        var prevInterStatus = PlayerStatusManager.GetPrevInterStatus();
        var currentInterStatus = PlayerStatusManager.GetCurrentInterStatus();

        switch(prevInterStatus)
		{
            case InteractionStatus.None:
                ActivateIcon(fIconObj, false);
                ActivateIcon(photoIconObj, false);
                break;
        }

        switch(currentInterStatus)
		{
            case InteractionStatus.None:
                HintPlayerPhotographicEvidence(ObjectSorter.CHPointingObj);
                ActivateIcon(investigationIconObj, false);
                ActivateIcon(walkieTalkieIconObj, false);
                ActivateIcon(npcIconObj, false);
                break;

            case InteractionStatus.TalkingNpc:
                ActivateIcon(npcIconObj, true);

                break;
            case InteractionStatus.TalkingWalkieTalkie:
                ActivateIcon(investigationIconObj, false);
                //ChangeTextOfIcon(walkieTalkieIconObj, hotKeyInfo.HotKeyDic[HotKey.Observe].ToString().ToUpper());
                ActivateIcon(walkieTalkieIconObj, true);

                break;
            case InteractionStatus.Investigating:
                ///ChangeTextOfIcon(exitIconObj, hotKeyInfo.HotKeyDic[HotKey.Observe].ToString().ToUpper());
                 ActivateIcon(walkieTalkieIconObj, false);
                ActivateIcon(investigationIconObj, true);


                break;
        }
    }

    

    void HintPlayerPhotographicEvidence(IObjectInfo pointingObject)
    {
        //Debug.Log(pointingObject.objTransform + " : " + pointingObject.objType);
        if (pointingObject.ObjTransform == null)
        {
            ActivateIcon(photoIconObj, false);
            return;
        }

        var evidenceInfos = pointingObject.ObjTransform.GetComponentsInChildren<PhotoEvidenceInfo>();
        foreach (var evidenceInfo in evidenceInfos)   // 어차피 한 오브젝트의 하위 객체들을 검사하는 거기 때문에, 하나만 포함되면 된다.
        {
            if (evidenceInfo.EvidenceType.name.Contains("Photo") && ! evidenceInfo.EvidenceType.IsObtained) //evidenceInfo.gameObject.activeSelf)
			{
                ActivateIcon(photoIconObj, true);
                return;
            }
        }

        ActivateIcon(photoIconObj, false);
        
    }

    void ChangeTextOfIcon(GameObject IconObj, string keyText)
	{
        if(IconObj.transform.TryGetComponent<TextMeshProUGUI>(out var iconTmPro))
            iconTmPro.text = keyText;
	}



}
