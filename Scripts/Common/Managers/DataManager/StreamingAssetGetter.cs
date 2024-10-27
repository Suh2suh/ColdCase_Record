using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class StreamingAssetGetter
{
	// communicatingNpc 접촉하면, Folder 접근


	protected List<string> LoadAllFileContentsInFolder(string folderStructure)
	{
		var filePaths = GetAllFilePathsInFolder(folderStructure);
		if (filePaths == null) return null;

		List<string> fileContents = new();

		foreach(var filePath in filePaths)
		{
			if(Path.GetExtension(filePath) != ".meta")   fileContents.Add(LoadFileContent(filePath));
		}

		return fileContents;
	}

	protected string LoadFileContent(string filePath)
	{
		if(File.Exists(filePath))
		{
			string fileContent = File.ReadAllText(filePath);
			return fileContent;
		}
		else
		{
			Debug.LogWarning("[NOTFOUND] There is no such filePath: " + filePath + "!");
			return null;
		}
	}



	public List<string> GetAllFilePathsInFolder(string folderStructure)
	{
		//Debug.Log("Getting All File Paths...");
		if (!Directory.Exists(GetFolderPath(folderStructure))) return null;

		string[] filePaths = Directory.GetFiles(GetFolderPath(folderStructure));
		List<string> filteredFilePaths = filePaths
														  .Where(file => Path.GetExtension(file) != ".meta")
														  .Select(file => file.Replace('\\', '/'))
														  .ToList<string>();

		return filteredFilePaths;
	}

	public string GetFolderPath(string folderStructure)
	{
		return Path.Combine(Application.streamingAssetsPath + folderStructure);
	}

}