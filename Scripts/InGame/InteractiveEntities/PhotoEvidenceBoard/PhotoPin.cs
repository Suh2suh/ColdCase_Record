using UnityEngine;
using UnityEngine.UI;

public class PhotoPin : MonoBehaviour
{
	[Tooltip("This name must be same as photographic evidence's <EvidenceInformation.evidenceName>")]
	[SerializeField] Evidence photoEvidenceType;  //JackPhoto, RestroomPhoto...
	public Evidence PhotoEvidenceType { get => photoEvidenceType; }


	#region Initialization

	private Transform photo;
	[HideInInspector] public GameObject glitchPlane;
	[HideInInspector] public GameObject canvas;

	private void Awake()
	{
		photo = GetComponentInChildren<InteractiveEntityInfo>().transform;
		glitchPlane = photo.Find("GlitchPlane").gameObject;
		canvas = photo.Find("Canvas").gameObject;
	}


	#endregion


	public void UpdatePhoto(Texture2D photoTexture)
	{		
		GetComponentInChildren<RawImage>().texture = photoTexture;

		var photoInspectorKey = photoEvidenceType.name;
		photo.GetComponentInChildren<ConditionalObjInspector>().SetItemKey(photoInspectorKey);
	}


	public void ShowPhotoNHideGlitch()
	{
		if (glitchPlane.activeSelf) glitchPlane.SetActive(false);
		if (!canvas.activeSelf) canvas.SetActive(true);
	}

	public void HidePhotoNShowGlitch()
	{
		if (!glitchPlane.activeSelf) glitchPlane.SetActive(true);
		if (canvas.activeSelf) canvas.SetActive(false);
	}


}
