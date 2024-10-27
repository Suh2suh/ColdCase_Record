using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



// TODO: 굳이 info로 받아올 필요가 없음

[CreateAssetMenu(fileName = "PhotoEvidenceManager", menuName = "ScriptableObjects/ManagersPhotoEvidenceManager", order = 2)]
public class PhotoEvidenceManager : ScriptableObject
{
	// TODO: 나중에는 Dictionary<Place, List<PhotoPin> 으로 Place 별 관리 혹은
	//             각 Place별 다른 Manager 만들어서 관리하든지 할 것
	//             현재로서, 어차피 photoEvidenceDic에는 찍은 사진 evidenceName이랑 Texutre가 들어가지만,
	//				object가 아니라서 자꾸 사라짐.
	// -> 그냥 Udictionary<Place, List<string>> takenPhotoNamePerPlace로 하고,
	//      불러올 때 해당 string으로 주소 불러올 것

	[SerializeField] PlayerInfo playerInfo;

	public UDictionary<string, Evidence> photoEvidenceDic;
	//public UDictionary<string, bool> photoEvidenceDic;
	Texture2D latestTextureFormat;

	public static System.Action<string, Texture2D> OnPhotoEvidenceDicUpdated;

	
	public void UpdatePhotoEvidenceDic(Evidence photoEvidenceType, Texture2D PhotoTexture)
	{
		latestTextureFormat = new Texture2D(PhotoTexture.width, PhotoTexture.height, PhotoTexture.format, PhotoTexture.mipmapCount > 1);
		Texture2D textureToUse = new Texture2D(PhotoTexture.width, PhotoTexture.height, PhotoTexture.format, PhotoTexture.mipmapCount > 1);
		Graphics.CopyTexture(PhotoTexture, textureToUse);
		textureToUse.Apply(false);

		SavePhotoEvidenceTexture(photoEvidenceType.name, textureToUse);


		if (photoEvidenceDic == null) photoEvidenceDic = new();

		if (photoEvidenceDic.ContainsKey(photoEvidenceType.name))
			photoEvidenceDic[photoEvidenceType.name] = photoEvidenceType;
			//photoEvidenceDic[photoEvidenceType.name] = textureToUse;
		else
			photoEvidenceDic.Add(photoEvidenceType.name, photoEvidenceType);

		OnPhotoEvidenceDicUpdated(photoEvidenceType.name, PhotoTexture);
		photoEvidenceType.SetIsObtained(true);


#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}


	public void ResetPhotoEvidenceDic()
	{
		foreach(var KeyphotoEvidencePair in photoEvidenceDic)
		{
			var photoEvidenceInfo = KeyphotoEvidencePair.Value;

			photoEvidenceInfo.SetIsObtained(false);
			photoEvidenceInfo.SetIsChecked(false);
		}

		photoEvidenceDic.Clear();



#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}


	void SavePhotoEvidenceTexture(string evidenceName, Texture2D evidenceTexture)
	{
		byte[] bytes = evidenceTexture.EncodeToJPG();
		File.WriteAllBytes(GetPhotoEvidencePath(evidenceName), bytes);

		Debug.Log(evidenceName + " is saved...! in " + GetPhotoEvidencePath(evidenceName));
	}


	public Texture2D LoadPhotoEvidenceTexture(string evidenceName)
	{
		if ( ! File.Exists(GetPhotoEvidencePath(evidenceName)))
		{
			Debug.Log(GetPhotoEvidencePath(evidenceName) + " doesn't exist!");

			return null;
		}


		Texture2D textureOutput = new Texture2D(1, 1);
		var textureContent = File.ReadAllBytes(GetPhotoEvidencePath(evidenceName));
		textureOutput.LoadImage(textureContent);


	

		return textureOutput;
	}


	public string GetPhotoEvidencePath(string evidenceName)
	{
		string fileName = "PhotoEvidence_" + evidenceName + ".jpg";

		//return Path.Combine(DataManager.GetPlaceDataFolderPath(playerInfo.CurrentPlace), fileName);
		return Path.Combine(DataManager.GetPlaceDataFolderPath(), fileName);
	}



}
