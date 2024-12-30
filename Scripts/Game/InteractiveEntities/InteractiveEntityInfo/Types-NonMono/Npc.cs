using UnityEngine;


[System.Serializable]
public class Npc
{
	[Tooltip("Npc Name Should be same as Npc's Sheet name in Dialogue")]
	[SerializeField] private string npcName = "";
	[SerializeField] private PlaceInfo npcPlace;


	#region Getters

	[HideInInspector] public string NpcName { get => npcName; }
	[HideInInspector] public PlaceInfo NpcPlace { get => npcPlace; }

	#endregion
}