using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogueKeyGetter
{
	public static string GetPrevKey(string key, UDictionary<string, CommunicationData> targetNpcData)
	{
		int currentKeyIndex = GetKeyIndexInData(key, targetNpcData);
		if (currentKeyIndex == 0) return "";

		int prevKeyIndex = currentKeyIndex - 1;
		return targetNpcData.Keys[prevKeyIndex];
	}


	/*
	/// <summary>  Return "" if prevKey is not valid  </summary>
	public static string GetNextKey(string prevKey, UDictionary<string, CommunicationData> targetNpcData)
	{
		if (NpcDialogueKeyChecker.IsKeyEndOfDialogue(prevKey, targetNpcData)) return "";

		if (NpcDialogueKeyChecker.IsDecisionKey(prevKey)) return GetResultKeyOfDecision(prevKey, targetNpcData);
		else if (NpcDialogueKeyChecker.IsResultKeyOfDecision(prevKey)) return GetNextKeyByKey(prevKey, targetNpcData);
		else return GetNextKeyByIndex(prevKey, targetNpcData);
	}*/


	/*
	public static string GetNextKeyByIndex(string key, UDictionary<string, CommunicationData> targetNpcData)
	{
		int currentKeyIndex = GetKeyIndexInData(key, targetNpcData);
		int nextKeyIndex = currentKeyIndex + 1;

		return targetNpcData.Keys[nextKeyIndex];
	}*/


	
	/// <summary>
	///  현재 키가 sentence일 시, 다음 sentence 불러옴. _text일 시, _0 불러옴. 다 없을 시, null 반환
	///  들어오는 애들: Phase_questionSubject_index, Phase_Error
	/// </summary>
	/// <param name="key"></param>
	/// <param name="targetNpcData"></param>
	/// <returns></returns>
	public static string GetNextKeyInNpcSheet(string key, UDictionary<string, CommunicationData> targetNpcData)
	{
		if (NpcDialogueKeyChecker.IsErrorKey(key) || key == "")   return null;


		var splits = key.Split("_");

		// A_Info_text -> A_Info
		// C_JackPhoto_0 -> C_JackPhoto
		string questionCode = splits[0] + "_" + splits[1];
		string sentenceIndex = splits[2];

		if(key.EndsWith("_text"))
		{
			if(NpcDialogueKeyChecker.IsKeyValid(questionCode + "_0", targetNpcData))   return questionCode + "_0";
		}
		else
		{
			if (NpcDialogueKeyChecker.IsKeyValid(questionCode + "_" + (int.Parse(sentenceIndex) + 1), targetNpcData))
				return questionCode + "_" + (int.Parse(sentenceIndex) + 1);
		}


		return null;
	}

	



	/*
	public static List<string> GetDecisionKeysFrom(string selectQuestionKey, UDictionary<string, CommunicationData> targetNpcData)
	{
		var newSelectQuestionKeys = new List<string>();
		newSelectQuestionKeys.Add(selectQuestionKey);

		var nextSelectKey = GetNextKeyByIndex(selectQuestionKey, targetNpcData);
		while (NpcDialogueKeyChecker.IsDecisionKey(nextSelectKey))
		{
			newSelectQuestionKeys.Add(nextSelectKey);
			nextSelectKey = GetNextKeyByIndex(nextSelectKey, targetNpcData);
		}

		return newSelectQuestionKeys;
	}

	public static  string GetResultKeyOfDecision(string selectKey, UDictionary<string, CommunicationData> targetNpcData)
	{
		if (targetNpcData.Keys.Contains(selectKey + DialogueInfo.selectErrorParse))
			return (selectKey + DialogueInfo.selectErrorParse);

		if (targetNpcData.Keys.Contains(selectKey + DialogueInfo.answerParse))
			return (selectKey + DialogueInfo.answerParse);


		return null;
	}

	public static string GetDecisionKeyOfDecisionResult(string decisionResultKey)
	{
		return decisionResultKey.Substring(0, decisionResultKey.Length - DialogueInfo.selectErrorParse.Length);
	}*/

	#region Advanced




	public static int GetKeyIndexInData(string key, UDictionary<string, CommunicationData> targetNpcData) 
	{ 
		return targetNpcData.Keys.IndexOf(key); 
	}

	/// <summary>  Every dialogue key starts with its phase: check google spread sheet  </summary>
	public static char GetDialoguePhaseOfKey(string key)
	{
		if (key.Length == 0) return '\0';
		return key.ToCharArray()[0];
	}


	public static CharacterCode GetCharacterCode(string key, CommunicationData communicationData)
	{
		return communicationData.characterCode;
	}

	#endregion
}