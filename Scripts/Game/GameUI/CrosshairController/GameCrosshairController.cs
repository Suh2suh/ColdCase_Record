﻿using System.Collections;
using TMPro;
using UnityEngine;

public class GameCrosshairController : CrosshairController
{
    [Header("Highlighted Crosshair Setting")]
    Vector3 defaultCHScale;
    [SerializeField, Space(15)] Sprite highlightCHSpriteCircle; 
    [SerializeField] Sprite highlightCHSpriteCheck;
    [SerializeField] Vector3 highlightCHScale;    
    [SerializeField] float upScaleDuration = 0.5f;   

    [Header("Detected Object Interact KeyHint")]
    [SerializeField] HotKeyInfo hotKeyInfo;
    [SerializeField] TextMeshProUGUI keyHintText;  
    Transform keyHintTransform; 

    bool isCHDefaultMode; 



	#region Unity Methods

	protected override void Awake()
	{
        ObjectSorter.OnCHPointingObjChanged += OnCHPointingObjChanged;
        PlayerStatusManager.OnInteractionStatusUpdated += OnInteractionStatusUpdated;

        base.Awake();
        defaultCHScale = ImageCrossHair.transform.localScale;
        keyHintTransform = keyHintText.transform;

        isCHDefaultMode = true;
    }

	private void Start()
	{
        ActivateCH();
        RevertCHColor();
        ZeroScaleCHKeyHint();
    }

    private void Update()
    {
        if (GameModeManager.GetCurrentGameMode() == GameMode.Game)
            ManageCHActivationUnder(PlayerStatusManager.GetCurrentInterStatus());
        else
            DeActivateCH();
    }

	private void OnDestroy()
	{
        ObjectSorter.OnCHPointingObjChanged -= OnCHPointingObjChanged;
        PlayerStatusManager.OnInteractionStatusUpdated -= OnInteractionStatusUpdated;
    }


    #endregion


    /// <summary>    Camera에서 Object 촬영 후, ObtainObject 획득 가능 상태로 변경할 때. Pointing Obj가 바뀌지 않기 때문에
    ///                        Manage를 한 번 더 업데이트 해주어야 함. </summary>
    void OnInteractionStatusUpdated()
	{
        var prevInterStatus = PlayerStatusManager.GetPrevInterStatus();
        if (prevInterStatus != InteractionStatus.Photo) return;

        RevertCH();
        ManageCHColorUnder(ObjectSorter.CHPointingObj);

    }

    void OnCHPointingObjChanged(ObjectInfo pointingObject)
	{
        //Debug.Log(pointingObject.objTransform + " " + pointingObject.objType);

        RevertCH();

        ManageCHColorUnder(pointingObject);
    }



    public void ManageCHActivationUnder(InteractionStatus currentInterStatus)
    {
        if (currentInterStatus == InteractionStatus.None)
            ActivateCH();
        else
            DeActivateCH();
    }

    public void ManageCHColorUnder(ObjectInfo pointingObj)
    {
        if (pointingObj.objType == ObjectType.None)   return;

        var chPointingObjType = pointingObj.objType;
        bool interactiveStatus = pointingObj.objTransform.GetComponent<InteractiveEntityInfo>().IsInteractive;
        //Debug.Log(pointingObj.objType + " " + interactiveStatus);

        switch (chPointingObjType)
        {
            case ObjectType.ObservableObj:
            case ObjectType.InteractiveFurniture:
            case ObjectType.ObservablePlace:
            case ObjectType.DetectiveDesk:
            case ObjectType.NPC:
                HighlightCH(Color.red, interactiveStatus);

                break;
            case ObjectType.ObtainableObj:
                HighlightCH(Color.green, interactiveStatus);

                break;
            //default:
                //RevertCHColor();
                //
                //break;
        }
    }



    #region Highlight / Revert CrossHair


    void HighlightCH(Color highlightColor, bool isInteractive = true)
    {
        if (isCHDefaultMode)
        {
            isCHDefaultMode = false;

            string keyHint = (highlightColor == Color.red ? hotKeyInfo.HotKeyDic[HotKey.Observe] : hotKeyInfo.HotKeyDic[HotKey.Obtain]).ToString();
            if (!isInteractive) keyHint = "<s>" + keyHint + "</s>";
            keyHintText.text = keyHint.ToUpper();

            ImageCrossHair.sprite = (isInteractive ? highlightCHSpriteCircle : highlightCHSpriteCheck);
            Color newCHColor = (isInteractive ? new Color(highlightColor.r, highlightColor.g, highlightColor.b, defaultCHColor.a) : defaultCHColor);
            ChangeCHColor(newCHColor);

            StartCoroutine(UpScaleCH());
        }

    }

    void RevertCH()
    {
        if (!isCHDefaultMode)
        {
            isCHDefaultMode = true;

            ImageCrossHair.sprite = defaultCHSprite;
            ImageCrossHair.transform.localScale = defaultCHScale;
            RevertCHColor();

            ZeroScaleCHKeyHint();
        }
    }



    IEnumerator UpScaleCH()
    {
        float time = 0;

        while (time < upScaleDuration)
        {
            if (isCHDefaultMode) break;

            ImageCrossHair.transform.localScale = Vector3.Lerp(defaultCHScale, highlightCHScale, time / upScaleDuration);
            keyHintTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / upScaleDuration);

            time += Time.unscaledDeltaTime;

            yield return null;
        }
        if (!isCHDefaultMode)
        {
            ImageCrossHair.transform.localScale = highlightCHScale;
            FullScaleKeyHint();
        }

        yield return null;
    }



    #endregion



    #region Activation Control

    public override void ActivateCH()
	{
		base.ActivateCH();
        ActivateKeyHintText(true);
    }
	public override void DeActivateCH()
	{
		base.DeActivateCH();
        ActivateKeyHintText(false);
    }



    void ActivateKeyHintText(bool activeStatus)
    {
        if (keyHintTransform.gameObject.activeSelf != activeStatus)   keyHintTransform.gameObject.SetActive(activeStatus);
    }

    void ZeroScaleCHKeyHint()
    {
        if (keyHintTransform)   keyHintTransform.localScale = Vector3.zero;
        //Debug.Log("Zero");
    }
    void FullScaleKeyHint()
    {
        if (keyHintTransform)   keyHintTransform.localScale = Vector3.one;
        //Debug.Log("One");
    }


	#endregion



}