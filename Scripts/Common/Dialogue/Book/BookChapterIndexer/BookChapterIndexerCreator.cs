using ColdCase.Dialogue.Book.Object;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCase.Dialogue.Book.Utility
{
	public class BookChapterIndexerCreator: MonoBehaviour
	{
		// TODO: 나중에 색 모듈화 -> ChapterNavigator에서? selected Indexer, else Indexer Color 지정하도록 하기
		readonly int indexerLayer = 10;   // this should be match with navigator's indexer layer

		[SerializeField, Space(15)] Transform chapterIndexerFolder;
		[SerializeField] GameObject chapterIndexerPrefab;
		[SerializeField] float indexerGap;
		float indexerHeight;

		Dictionary<string, BookChapterIndexer> bookChapterIndexerDic;
		public Dictionary<string, BookChapterIndexer> BookChapterIndexerDic { get => bookChapterIndexerDic; }
		


		private void Awake()
		{
			if(bookChapterIndexerDic == null) Initialize();
		}


		public BookChapterIndexer CreateNpcIndexer(string chapterName)
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

			var indexerController = indexer.GetComponent<BookChapterIndexer>() ?? indexer.AddComponent<BookChapterIndexer>();
			indexerController.Initialize(chapterName);

			bookChapterIndexerDic[chapterName] = indexerController;

			return indexerController;
		}

		public BookChapterIndexer CreateNpcIndexer(string chapterName, Color indexerColor)
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

			var indexerController = indexer.GetComponent<BookChapterIndexer>() ?? indexer.AddComponent<BookChapterIndexer>();
			indexerController.Initialize(chapterName, indexerColor);

			bookChapterIndexerDic[chapterName] = indexerController;

			return indexerController;
		}


		void Initialize()
		{
			if (chapterIndexerPrefab != null) indexerHeight = chapterIndexerPrefab.transform.localScale.y;
			bookChapterIndexerDic = new();
		}

	}
}