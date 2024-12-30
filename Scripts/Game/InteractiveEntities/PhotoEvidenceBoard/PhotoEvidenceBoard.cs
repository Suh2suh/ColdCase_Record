using System.Collections.Generic;
using UnityEngine;


public class PhotoEvidenceBoard : MonoBehaviour
{
	[SerializeField] private PhotoEvidenceManager photoEvidenceManager;
	[SerializeField] private List<PhotoPin> PickedPhotoPins;


	#region Unity Methods

	private void Awake()
	{
		PhotoEvidenceManager.OnPhotoEvidenceDicUpdated += OnPhotoEvidenceDicUpdated;
	}

	private void Start()
	{
		LoadExistingPhotoEvidences();
	}

	private void OnDestroy()
	{
		PhotoEvidenceManager.OnPhotoEvidenceDicUpdated -= OnPhotoEvidenceDicUpdated;
	}


	#endregion


	private void LoadExistingPhotoEvidences()
	{
		foreach (var photoPin in PickedPhotoPins)
		{
			if (photoPin.PhotoEvidenceType.IsObtained)
			{
				Texture2D photoTexture = photoEvidenceManager.LoadPhotoEvidenceTexture(photoPin.PhotoEvidenceType.name);

				if (photoTexture)
				{
					photoPin.ShowPhotoNHideGlitch();
					photoPin.UpdatePhoto(photoTexture);
				}
				else
				{
					photoPin.HidePhotoNShowGlitch();
				}
			}
		}

	}


	private void OnPhotoEvidenceDicUpdated(string evidenceName, Texture2D PhotoTexture)
	{
		foreach (PhotoPin photoPin in PickedPhotoPins)
		{
			if (photoPin.PhotoEvidenceType.name == evidenceName)
			{
				photoPin.ShowPhotoNHideGlitch();
				photoPin.UpdatePhoto(PhotoTexture);
			}
		}
	}


}
