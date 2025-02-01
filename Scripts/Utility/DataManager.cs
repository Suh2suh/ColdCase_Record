using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public static class DataManager
{
    public static string settingDataPath = Path.Combine(Application.persistentDataPath, "settingData.json");
    public static bool isSettingDataExists = File.Exists(DataManager.settingDataPath);   // TODO: 추후 더 좋은 클래스 구성을 할 수 있을 것 같음. 일단 임시로 두기

    // Path\Save
    public static string saveDataPath = Path.Combine(Application.persistentDataPath, "Save");
    /// <summary>  Path\Save\PlaceName  </summary>
    public static string GetPlaceDataFolderPath(PlaceInfo placeInfo)
	{
        return Path.Combine(saveDataPath, placeInfo.name);
        //return Path.Combine(saveDataPath, "Tutorial");
	}
    public static string GetPlaceDataFolderPath()
    {
        //return Path.Combine(saveDataPath, placeInfo.name);
        return Path.Combine(saveDataPath, "Tutorial");
    }


    public static string ReadData(string dataPath)
	{
        string loadedJson = "";
        using (StreamReader streamReader = new(dataPath))
        {
            loadedJson = streamReader.ReadToEnd();
        }

        return loadedJson;
    }

    //public static void WriteData(string dataPath, string dataContent)
    public static void WriteData(string dataPath, string dataContent)
    {
        string directory = Path.GetDirectoryName(dataPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        using (StreamWriter streamWriter = new(dataPath))
        {
            streamWriter.Write(dataContent);
            //yield return streamWriter.WriteAsync(dataContent);
        }


        //Debug.Log("Successfully Saved Setting Data: " + dataPath);
        //Debug.Log(dataContent);
    }




    const string dialogueGoogleSheetUrl = "https://docs.google.com/spreadsheets/d/16yIPwmEwW4iUV_bTm8uX2T_DQ33_KzkAmolukJVNV98";
    // CSV
    public static Dictionary<string, string> dialogueUrlDictionary = new()
	{
        // Common
        { "Narration", dialogueGoogleSheetUrl + "/export?format=csv&gid=847918344" },
        { "WalkieTalkie", dialogueGoogleSheetUrl + "/export?format=csv&gid=599518870" },

        { "ItemName", dialogueGoogleSheetUrl + "/export?format=csv&gid=2066089264" },
        { "ItemExplanation", dialogueGoogleSheetUrl + "/export?format=csv&gid=1197069729" },

        { "Book", dialogueGoogleSheetUrl + "/export?format=csv&gid=1247339386" },
        { "InvestigationEvent", dialogueGoogleSheetUrl + "/export?format=csv&gid=254974937" },

        { "Setting", dialogueGoogleSheetUrl + "/export?format=csv&gid=48815123" },



        // Npcs
        { "Prank", dialogueGoogleSheetUrl + "/export?format=csv&gid=0" },
        { "Miranda", dialogueGoogleSheetUrl + "/export?format=csv&gid=2008344363" },
        { "Laura", dialogueGoogleSheetUrl + "/export?format=csv&gid=195613914" },
    };

}
