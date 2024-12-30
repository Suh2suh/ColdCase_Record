using ColdCase.Dialogue.Book.Object;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCase.Dialogue.Book.Utility
{
	public class BookChapterIndexerCreator: MonoBehaviour
	{
		#region Setting Variables
		[SerializeField, Space(15)] private Transform chapterIndexerFolder;
		[SerializeField] private GameObject chapterIndexerPrefab;
		[SerializeField] private float indexerGap;

		#endregion

		#region Private Variables
		private readonly int indexerLayer = 10;   // this should be match with navigator's indexer layer
		private float indexerHeight;
		private Dictionary<string, BookChapterIndexer> bookChapterIndexerDic;

		#endregion
		public Dictionary<string, BookChapterIndexer> BookChapterIndexerDic { get => bookChapterIndexerDic; }


		#region Initialization

		private void Awake()
		{
			if(bookChapterIndexerDic == null) Initialize();
		}

		private void Initialize()
		{
			if (chapterIndexerPrefab != null) indexerHeight = chapterIndexerPrefab.transform.localScale.y;
			bookChapterIndexerDic = new();
		}


		#endregion


		public BookChapterIndexer CreateNpcIndexer(string chapterName)
		{
			var indexer = InstantiateNpcIndexer(chapterName);
			if (indexer == null)  return null;

			var indexerController = indexer.GetComponent<BookChapterIndexer>() ?? indexer.AddComponent<BookChapterIndexer>();
			indexerController.Initialize(chapterName);

			bookChapterIndexerDic[chapterName] = indexerController;

			return indexerController;
		}

		public BookChapterIndexer CreateNpcIndexer(string chapterName, Color indexerColor)
		{
			var indexer = InstantiateNpcIndexer(chapterName);
			if (indexer == null)  return null;

			var indexerController = indexer.GetComponent<BookChapterIndexer>() ?? indexer.AddComponent<BookChapterIndexer>();
			indexerController.Initialize(chapterName, indexerColor);

			bookChapterIndexerDic[chapterName] = indexerController;

			return indexerController;
		}


		private GameObject InstantiateNpcIndexer(string chapterName)
		{
			if (chapterIndexerPrefab == null || chapterIndexerFolder == null)
			{
				Debug.LogWarning("[Prefab Instantiation Failed] Prefab or Folder is Empty!");
				return null;
			}
			if (bookChapterIndexerDic == null) Initialize();


			var indexer = Instantiate(chapterIndexerPrefab, chapterIndexerFolder, false);
			var indexerCreationPos = new Vector3(0, -(chapterIndexerFolder.childCount - 1) * (indexerHeight + indexerGap), 0);
			indexer.transform.localPosition = indexerCreationPos;
			indexer.name = chapterName;
			if (indexer.layer != indexerLayer) indexer.layer = indexerLayer;

			return indexer;
		}


	}
}