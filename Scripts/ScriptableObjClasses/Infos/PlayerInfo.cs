using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerInfo", menuName = "ScriptableObjects/Informations/PlayerInfo", order = 1)]
public class PlayerInfo : ScriptableObject
{
	[SerializeField] PlaceInfo currentPlace;
	[HideInInspector] public PlaceInfo CurrentPlace 
	{
		get => currentPlace; 
		set
		{
			currentPlace = value;
			//Debug.Log("hi" + currentPlace);


		}
	}




	[SerializeField] List<PlaceInfo> gamePlaceInSequence;
	[HideInInspector] public List<PlaceInfo> GamePlaceInSequence { get => gamePlaceInSequence; }

}
