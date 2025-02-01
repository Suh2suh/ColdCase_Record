using UnityEngine;


[System.Serializable]
public class Npc

{
	#region Setting Variables
	[Tooltip("Npc Name Should be same as Npc's Sheet name in Dialogue")]
	[SerializeField] private string npcName = "";
	[SerializeField] private PlaceInfo npcPlace;

	#endregion

	public string NpcName { get => npcName; }
	public PlaceInfo NpcPlace { get => npcPlace; }

}