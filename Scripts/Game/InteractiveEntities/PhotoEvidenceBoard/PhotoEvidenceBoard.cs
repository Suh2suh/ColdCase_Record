using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PhotoEvidenceBoard : MonoBehaviour
{
	[SerializeField] PhotoEvidenceManager photoEvidenceManager;
	[SerializeField] List<PhotoPin> PickedPhotoPins;


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


	void LoadExistingPhotoEvidences()
	{
		foreach (var photoPin in PickedPhotoPins)
		{
			Debug.Log(photoPin.name);

			//if (photoEvidenceManager.photoEvidenceDic.ContainsKey(photoPin.PhotoEvidenceType.name))
			if (photoPin.PhotoEvidenceType.IsObtained)
			{
				Texture2D photoTexture = photoEvidenceManager.LoadPhotoEvidenceTexture(photoPin.PhotoEvidenceType.name);
				Debug.Log(photoPin.PhotoEvidenceType.name + ": " + photoTexture);

				if (photoTexture)
				{
					photoPin.ShowPhotoNHideGlitch();
					photoPin.UpdatePhoto(photoTexture);
				} else
				{
					photoPin.HidePhotoNShowGlitch();
				}
			}

		}
	}



	void OnPhotoEvidenceDicUpdated(string evidenceName, Texture2D PhotoTexture)
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



	//public void ResetTargetPhase()
	//{
		//foreach(var photoPin in PickedPhotoPins)
		//{
			//var npcPhaseUpdater = photoPin.GetComponentInChildren<PEProgressUpdater>();

			//npcPhaseUpdater.ResetTargetPhase();
		//}
	//}


}
