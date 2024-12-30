using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using IE.RichFX;
using TRS.CaptureTool;


/// <summary> Attatch this to any object  </summary>
public class PhotoMaker : MonoBehaviour
{
	#region Setting Variables
	[Header("UIs")]
    [SerializeField] GameObject CameraPanel;
    [SerializeField] GameObject CameraFramePanel;
    [SerializeField] GameObject PhotoResultPanel;
    [SerializeField] Image whitePanelImage;
    [SerializeField] Image cameraRayAreaImage;

    [Header("Related Classes")]
    [SerializeField] TRS.CaptureTool.ScreenshotScript screenshotScript;
    [SerializeField] PhotoEvidenceManager photoEvidenceManager;
    [SerializeField] CrosshairController cameraCrosshairController;
    [SerializeField] Volume post_effect_volume;
    [SerializeField] TutorialInfo tutorialInfo;

	#endregion

	#region Private Variables
	Vector3 originalPhotoResultPos;
	Quaternion originalPhotoResultRot;

	#endregion

	/// <summary>  
    /// 플레이어 이동 제한에 영향: (촬영 시작 ? 제한 O : 제한 X) 
    /// </summary>
	public static bool isPhotoTaking = false;


	#region Screenshot Delegate Connection/DeConnection

	public PhotoMaker()
	{
        ScreenshotScript.ScreenshotTaken += ScreenshotTaken;

    }

    ~PhotoMaker()
	{
        ScreenshotScript.ScreenshotTaken -= ScreenshotTaken;
    }


	#endregion

	#region Unity Methods

	private void Start()
	{
        // Camera Effect + Screenshot
        DeActivateCameraPanel();
        DeActivateCameraFrame();

        HideWhitePanel();
        if (!whitePanelImage.gameObject.activeSelf) whitePanelImage.gameObject.SetActive(true);
        if (PhotoResultPanel.activeSelf) PhotoResultPanel.SetActive(false);

        originalPhotoResultPos = PhotoResultPanel.GetComponent<RectTransform>().position;
        originalPhotoResultRot = PhotoResultPanel.GetComponent<RectTransform>().rotation;
    }


	void Update()
    {
        // 카메라 안 들었을 때 - 카메라 들지 확인
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None && tutorialInfo.IsTutorialEnd)
        {
            if (HotKeyChecker.isKeyPressed[HotKey.Photo])
			{
                isPhotoTaking = false;

                ActivateCameraPanel();
                ActivateCameraFrame();

                InitializeRaysInArea(CameraPanel.GetComponentInParent<CanvasScaler>().scaleFactor);
                isPhotoRayNeeded = true;

                PlayerStatusManager.SetInterStatus(InteractionStatus.Photo);
            }
        }

        // 카메라 들었을 때 - 카메라 놓을지 확인
        else if(PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Photo)
        {
            if (HotKeyChecker.isKeyPressed[HotKey.Photo] || HotKeyChecker.isKeyPressed[HotKey.Escape])
			{
                if (!isPhotoTaking)
                {
                    DeActivateCameraFrame();
                    isPhotoRayNeeded = false;

                    PlayerStatusManager.SetInterStatus(InteractionStatus.None);
                }
            }
        }


        // 카메라 들었을 때 - 레이 체크 (놓고 사진 찍는 키 동시 누를 때 버그 발생 해결 위해 이렇게 분리)
        if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.Photo)
		{
            if(!isPhotoTaking)
			{
                ControlCameraCHColor();

                if (Input.GetMouseButtonDown(0))
                {
                    isPhotoTaking = true;

                    DeActivateCameraFrame();
                    ShowWhitePanel();

                    isPhotoRayNeeded = false;

                    StartCoroutine(FadeOut(whitePanelImage, 0.5f));
                }
            }
		}
    }


	private void FixedUpdate()
	{
        if(isPhotoRayNeeded)
            DetectPhotographicEvidence();
    }


	#endregion



    #region Camera Shooting Effect & Take Screenshot

    [Header("Camera Shooting Effect")]
    [SerializeField] float photoViewTime = 3f;
    [SerializeField] float photoDownDuration = 0.5f;

    float fadeStage = 10f;


    IEnumerator FadeOut(Image FadeImage, float finalAlpha)
	{
        float lerpDuration = (1.0f - finalAlpha) / fadeStage;
        float lerpT = 0f;

        while(lerpT < 1.0f)
		{
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, Mathf.Lerp(1.0f, finalAlpha, lerpT));

            lerpT += lerpDuration;

            yield return null;
        }

        HideWhitePanel();

        // 스크린샷 툴 에셋 함수 실행
        screenshotScript.TakeSingleScreenshot(false);
        yield return new WaitForSecondsRealtime(photoViewTime);

        StartCoroutine(PutPhotoResultDown());
    }


	// screenshotScript.TakeSingleScreenshot() -> 스크린샷 툴 에셋이 ScreenshotTaken 이벤트 발생시킴
	void ScreenshotTaken(ScreenshotScript screenshotScript, Texture2D screenshotTexture)
    {
        var detectedEvidenceExists = (detectedPhotographicEvidences.Count > 0);
        if (detectedEvidenceExists)
        {
            Texture2D textureToUse = new Texture2D(screenshotTexture.width, screenshotTexture.height, screenshotTexture.format, screenshotTexture.mipmapCount > 1);
            Graphics.CopyTexture(screenshotTexture, textureToUse);
            textureToUse.Apply(false);

            foreach (var photographicEvidence in detectedPhotographicEvidences)
            {
                var detectedPhotoEvidenceType = photographicEvidence.GetComponent<PhotoEvidenceInfo>().EvidenceType;

                if ( ! detectedPhotoEvidenceType.IsObtained)
                    SetInteractiveIfObtainableObj(photographicEvidence);

                photoEvidenceManager.UpdatePhotoEvidenceDic(detectedPhotoEvidenceType, textureToUse);
            }

            detectedPhotographicEvidences.Clear();
        }
    }

    void SetInteractiveIfObtainableObj(Transform photographicEvidence)
	{
        var obtainableObjCandidate = photographicEvidence.GetComponent<InteractiveEntityInfo>();
        if (obtainableObjCandidate != null && obtainableObjCandidate.ObjectType == ObjectType.ObtainableObj)
        {
			obtainableObjCandidate.IsInteractive = true;
		}
    }


	    #region Control PhotoResultPanel Position

	    IEnumerator PutPhotoResultDown()
	    {
            float time = 0f;

            float randomZRot = Random.Range(15, 25);
            var finalPos = new Vector3(-20, -20, 0);
            var finalRot = Quaternion.Euler(0, 0, randomZRot);
            var photoResultPanelRect = PhotoResultPanel.transform.GetComponent<RectTransform>();

			while (time < photoDownDuration)
            {
                var lerpT = time / photoDownDuration;

			    photoResultPanelRect.position = Vector3.Lerp(originalPhotoResultPos, finalPos, lerpT);
                photoResultPanelRect.rotation = Quaternion.Lerp(originalPhotoResultRot, finalRot, lerpT);
                PhotoResultPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, lerpT);

                time += 0.01f;

                yield return null;
            }
            photoResultPanelRect.position = finalPos;
            photoResultPanelRect.rotation = finalRot;
            PhotoResultPanel.transform.localScale = Vector3.zero;


            PhotoResultPanel.SetActive(false);
            TurnPhotoResultBack();

            isPhotoTaking = false;
            PlayerStatusManager.SetInterStatus(InteractionStatus.None);
        }

        void TurnPhotoResultBack()
	    {
            PhotoResultPanel.transform.SetPositionAndRotation(originalPhotoResultPos, originalPhotoResultRot);
            PhotoResultPanel.transform.localScale = Vector3.one;

        }


    #endregion


    #endregion


    #region Photo Ray Check

    [Header("Photographic Evidence Detection")]
    [SerializeField] float rayReachDistance = 10.0f;

	bool isPhotoRayNeeded = false;

    List<Ray> raysInArea = new();
    List<Transform> detectedPhotographicEvidences = new();

    Vector2 screenCenter;
    Ray prevCenterRay;

    int photoLayerMask = 1 << 7;


    /// <summary> 
    /// Detect Photographic Evidence with camera + update "detectedPhotographicEvidence"List  
    /// </summary>
    void DetectPhotographicEvidence()
    {
        if(detectedPhotographicEvidences.Count > 0)  detectedPhotographicEvidences.Clear();


        UpdateRaysIfCamMoved();
        if (!CheckEvidenceSeenOnCamera())  return;


        foreach(var ray in raysInArea)
		{
            if (Physics.Raycast(ray, out RaycastHit hit, rayReachDistance, photoLayerMask))
            {
                var evidenceCandidate = hit.transform.GetComponent<PhotoEvidenceInfo>();

                if (evidenceCandidate == null || ! evidenceCandidate.EvidenceType.name.Contains("Photo"))
                    continue;

                if ( ! detectedPhotographicEvidences.Contains(hit.transform))
                    detectedPhotographicEvidences.Add(hit.transform);
            }
        }
    }


    void UpdateRaysIfCamMoved()
	{
        var currentCenterRay = Camera.main.ScreenPointToRay(screenCenter);
        if (prevCenterRay.origin != currentCenterRay.origin)
        {
            UpdateRaysInArea(currentCenterRay.origin - prevCenterRay.origin, currentCenterRay.direction - prevCenterRay.direction);

            prevCenterRay = currentCenterRay;
        }
    }

    void UpdateRaysInArea(Vector3 originChange, Vector3 dirChange)
    {
        for (int i = 0; i < raysInArea.Count; i++)
            raysInArea[i] = new Ray(raysInArea[i].origin + originChange, raysInArea[i].direction + dirChange);
    }


    bool CheckEvidenceSeenOnCamera()
	{
        Ray centerRay = raysInArea[raysInArea.Count / 2];

        if (Physics.Raycast(centerRay, out RaycastHit centerHit, rayReachDistance, photoLayerMask))
            return CameraViewportObjectChecker.CheckObjSeenOnCamera(centerHit.transform);

        return false;

    }


    /// <summary>  
    /// 화면에서 보이는 실제 이미지 크기 계산 위해 할당. CameraRayArea 이미지 영역 내 9개의 레이 발사 
    /// </summary>
    void InitializeRaysInArea(float canvasFactor)
    {
        // 현재 해상도에 비례한 실제 rayAreaImage Width/Height 구하기
        float rayAreaImageW = cameraRayAreaImage.rectTransform.rect.width * canvasFactor;
        float rayAreaImageH = cameraRayAreaImage.rectTransform.rect.height * canvasFactor;
        float halfW = rayAreaImageW / 2,   halfH = rayAreaImageH / 2;

        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        raysInArea.Clear();

        // update시 center의 변화량만 각 점에 더해주면 된다.
        for (int i = -1; i <= 1; i++)
            for (int k = -1; k <= 1; k++)
            {
                var screenPointInArea = new Vector2(screenCenter.x + halfW * i, screenCenter.y + halfH * k);
                raysInArea.Add(Camera.main.ScreenPointToRay(screenPointInArea));

                if (i == 0 && k == 0)   prevCenterRay = Camera.main.ScreenPointToRay(screenPointInArea);
            }

    }


    void ControlCameraCHColor()
    {
        bool detectedEvidenceExists = (detectedPhotographicEvidences.Count > 0);

        if (detectedEvidenceExists) cameraCrosshairController.ChangeCHColor(Color.red);
        else cameraCrosshairController.RevertCHColor();   // 여기서, default가 들어가지 않은 상태로 (0,0,0,0)으로 변경됨
    }


    #endregion


    #region Activate/DeActivate, Show/Hide UIs

    void ShowWhitePanel() { whitePanelImage.color = new Color(whitePanelImage.color.r, whitePanelImage.color.g, whitePanelImage.color.b, 1f); }
    void HideWhitePanel() { whitePanelImage.color = new Color(whitePanelImage.color.r, whitePanelImage.color.g, whitePanelImage.color.b, 0f); }

    void ActivateCameraPanel() { if (!CameraPanel.activeSelf) CameraPanel.SetActive(true); }
    void DeActivateCameraPanel() { if (CameraPanel.activeSelf) CameraPanel.SetActive(false); }

    void ActivateCameraFrame() { if (!CameraFramePanel.activeSelf) CameraFramePanel.SetActive(true); }
    void DeActivateCameraFrame() { if (CameraFramePanel.activeSelf) CameraFramePanel.SetActive(false); }


	#endregion

}
