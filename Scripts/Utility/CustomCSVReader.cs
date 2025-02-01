using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class CustomCSVReader
{
	#region Original Code

	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
//    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

    // 따옴표 안에 있는 개행문자는 무시
    static string LINE_SPLIT_RE = @"\n(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

    static char[] TRIM_CHARS = { '"' };
    //static char[] TRIM_CHARS = { '\"' };


    //public static List<Dictionary<string, object>> Read(string file)
    public static List<Dictionary<string, object>> Read(string csvText)
    {
        var list = new List<Dictionary<string, object>>();
        //TextAsset data = Resources.Load(file) as TextAsset;

        //var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        Debug.Log(csvText);

        var lines = Regex.Split(csvText, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);

        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            //Debug.Log(i + "번 라인");
            Debug.Log(lines[i]);

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                Debug.Log("[" + j + "번]");

                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;

                Debug.Log(header[j] + " : " + finalvalue);
            }
            list.Add(entry);
        }
        return list;
    }


	#endregion



	#region Custom Code

	public static UDictionary<string, UDictionary<Language, string>> ReadCommonDialogue(string csvText)
    {
        var commonDic = new UDictionary<string, UDictionary<Language, string>>();

        var lines = Regex.Split(csvText, LINE_SPLIT_RE);
        if (lines.Length <= 1) return commonDic;

        var header = Regex.Split(lines[0], SPLIT_RE);   // header: 0번째 열 (Key, Kor, Eng, Jap)

        
        for (var i = 1; i < lines.Length; i++)   // line: 각 행
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;   // value가 아예 없거나, key가 비어있으면 패스

            var key = values[0];


            var ValuePerlanguage = new UDictionary<Language, string>();

            for (var j = 1; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                var finalvalue = value; // 현재 value 값 후처리

                var language = (Language)(j - 1);
                ValuePerlanguage[language] = finalvalue;
            }

            commonDic.Add(key, ValuePerlanguage);

        }

        //Print(commonDic);

        return commonDic;
    }

    public static UDictionary<string, CommunicationData> ReadNpcDialogue(string csvText)
    {
        var npcDic = new UDictionary<string, CommunicationData>();

        var lines = Regex.Split(csvText, LINE_SPLIT_RE);
        if (lines.Length <= 1) return npcDic;

        var header = Regex.Split(lines[0], SPLIT_RE);


        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;   // value가 아예 없거나, key가 비어있으면 패스

            var key = values[0];


            var communicationData = new CommunicationData();

            string characterCodeValue = values[1];
            communicationData.characterCode = SortCharacterCode(characterCodeValue);

            string faceValue = values[2];
            communicationData.faceExpression = SortFaceExpression(faceValue);

            string pronounceValue = values[3];
            communicationData.pronounce = SortPronounce(pronounceValue);

            var ValuePerlanguage = new UDictionary<Language, string>();

            for (var j = 4; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                var finalvalue = value;

                var language = (Language)(j - 4);
                ValuePerlanguage[language] = finalvalue;
            }

            communicationData.dialogueByLanguage = ValuePerlanguage;
            npcDic.Add(key, communicationData);

        }

        //Print(npcDic);

        return npcDic;
    }


    static CharacterCode SortCharacterCode(string characterCodeStr)
    {
        switch (characterCodeStr)
        {
            case "Npc":
                return CharacterCode.Npc;

            case "Player":
                return CharacterCode.Player;

            default:
                return CharacterCode.None;
        }

    }


    static Face SortFaceExpression(string faceExpressionStr)
	{
        switch(faceExpressionStr)
		{
            case "Frown":
                return Face.Frown;

            case "Smile":
                return Face.Smile;

            case "Tearful":
                return Face.Tearful;

            default:
                return Face.None;
		}

	}

    static Pronounce SortPronounce(string pronounce)
    {
        switch (pronounce)
        {
            case "A":
                return Pronounce.a;

            case "E":
                return Pronounce.e;

            case "I":
                return Pronounce.i;

            case "O":
                return Pronounce.o;

            case "U":
                return Pronounce.u;

            default:
                return Pronounce.None;
        }

    }



    public static void PrintDialogueDic(UDictionary<string, UDictionary<Language, string>> commonDic)
    {
        foreach (var keyPair in commonDic)
        {
            Debug.Log(keyPair.Key);

            foreach (var dialoguePair in keyPair.Value)
            {
                Debug.Log(dialoguePair.Key + " : " + dialoguePair.Value);
            }

        }
    }

    public static void PrintDialogueDic(UDictionary<string, CommunicationData> npcDic)
    {
        foreach (var keyPair in npcDic)
        {
            Debug.Log(keyPair.Key);

            var communicatioinData = keyPair.Value;

            Debug.Log("face: " +  communicatioinData.faceExpression + " / pronounce: " + communicatioinData.pronounce);

            foreach(var dialoguePair in communicatioinData.dialogueByLanguage)
			{
                Debug.Log(dialoguePair.Key + " : " + dialoguePair.Value);
			}

        }
    }



    #endregion

}
