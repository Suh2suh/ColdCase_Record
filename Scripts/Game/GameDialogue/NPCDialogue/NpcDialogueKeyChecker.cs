using System.Collections;
using UnityEngine;

public class NpcDialogueKeyChecker
{
	#region Basic

	public static bool IsErrorKey(string key)
	{
		return (key.Contains(DialogueInfo.errorParse));
	}


	#endregion


	#region Advanced

	public static bool IsKeyStartOfPhase(string prevKey, string newKey)
	{
		return (NpcDialogueKeyGetter.GetDialoguePhaseOfKey(prevKey) != NpcDialogueKeyGetter.GetDialoguePhaseOfKey(newKey));
	}

	public static bool IsKeyEndOfDialogue(string key, UDictionary<string, CommunicationData> targetNpcData)
	{ 
		return (NpcDialogueKeyGetter.GetKeyIndexInData(key, targetNpcData) == targetNpcData.Count - 1); 
	}
	public static bool IsKeyValid(string key, UDictionary<string, CommunicationData> targetNpcData)
	{
		return (targetNpcData.Keys.Contains(key)); 
	}


	#endregion

}