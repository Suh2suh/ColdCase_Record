using ColdCase.Dialogue.Book;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ColdCase.Dialogue.Book.Utility;

namespace ColdCase.Dialogue.Book.Object
{
	public class Book : MonoBehaviour
	{
		[Header("Turn this OFF when you are not reading this book")]
		[SerializeField] private bool isBookReadable = true;

		[SerializeField, Space(15)]
		private List<BookChapter> bookChapters;
		[SerializeField, Tooltip("Input only when you do not want to do autoNaming")]
		private Transform initialChapter;
		[SerializeField, Tooltip("Chapter/Indexer will be numbering as arabic, chapter 0 would be first display chapter")]
		private bool isChapterAutoNaming = false;
		private bool isBookChapterValid;

		[Header("Book Chapter Navigation")]
		private BookChapterNavigator bookChapterNavigator;
		private bool isBookChapterMultiple = false;
		private bool isBookChapterNavigatable = false;

		[SerializeField]
		BookChapterIndexerCreator bookChapterIndexerCreator;

		[Header("Book Page Navigation")]
		private BookPageNavigator bookPageNavigator;
		[SerializeField, Space(15)] private GameObject leftButton;
		[SerializeField] private GameObject rightButton;

		private BookHighlighter bookMarker;
		[SerializeField, Space(15)] private bool isBookHighlightable = false;
		[SerializeField] private Color highlightColor;
		[SerializeField] private Vector4 highlightPadding;


		private Dictionary<string, BookChapter> bookChapterDic;
		private Dictionary<string, int> pageNavigationRecordOnChapter;


		#region Unity Methods

		private void Awake()
		{
			if (!isBookReadable) return;


			isBookChapterValid = (bookChapters != null && bookChapters.Count > 0);
			if (isBookChapterValid)
			{
				if (isChapterAutoNaming) AutoNameBookChapters(bookChapters);

				bookChapterDic = new();
				pageNavigationRecordOnChapter = new();
				// 추후, 챕터별로 첫 페이지 지정하고 싶다면, chapter에 페이지 추가해서 해당 문장에서 불러올 것
				foreach (var bookChapter in bookChapters)
				{
					bookChapterDic[bookChapter.name] = bookChapter;
					pageNavigationRecordOnChapter[bookChapter.name] = 0;
				}


				bool isBookPageValid = (bookChapters[0].BookPages != null && bookChapters[0].BookPages.Count > 0);
				if (isBookPageValid)
				{
					bookPageNavigator = new BookPageNavigator(leftButton, rightButton, bookChapters[0], InitialnavigatingPageIndex: 0);
					bookPageNavigator.OnPageFlipped += OnPageFlipped;
				}
			}


			isBookChapterMultiple = (bookChapters != null && bookChapters.Count > 1);
			if (isBookChapterMultiple)
			{
				bookChapterNavigator = (initialChapter == null ? new BookChapterNavigator(GetGameObjList(bookChapters), 0) :
																								 new BookChapterNavigator(GetGameObjList(bookChapters), initialChapter.name));

				isBookChapterNavigatable = (bookChapterNavigator != null && bookChapterIndexerCreator != null);
				if (isBookChapterNavigatable) bookChapterNavigator.OnChapterFlipped += OnChapterFlipped;
			}


			if (isBookHighlightable)
			{
				bookMarker = new BookHighlighter_Mark(highlightColor, highlightPadding);

				foreach (var chapter in bookChapters)
					foreach (var bookPage in chapter.BookPages)
					{
						if(bookPage == null) continue;

						foreach (var textPair in bookPage.ApplyingKeyPerTargetText)
							bookMarker.LinkHighlightRecieverTo(textPair.Key.transform, textPair.Value.sheetName);
					}
			}
		}

		private void Start()
		{
			if (!isBookReadable) return;


			if (isBookChapterNavigatable)
			{
				foreach (var bookChapter in bookChapters) 
					bookChapterIndexerCreator.CreateNpcIndexer(bookChapter.name);
			}		
		}

		private void FixedUpdate()
		{
			if (!isBookReadable) return;


			if (isBookChapterNavigatable) 
				bookChapterNavigator.CheckPointingIndexerOnFixedUpdate();
		}

		private void Update()
		{
			if (!isBookReadable) return;


			if (isBookHighlightable) bookMarker.ManageTextHighlightOnUpdate();

			if (isBookChapterNavigatable) bookChapterNavigator.ManageChapterNavigationOnUpdate();

		}

		private void OnDestroy()
		{

			if (isBookChapterNavigatable) bookChapterNavigator.OnChapterFlipped -= OnChapterFlipped;
		}


		#endregion


		private void AutoNameBookChapters(List<BookChapter> bookChapters)
		{
			for (int chapterIndex = 0; chapterIndex < bookChapters.Count; chapterIndex++)
			{
				var bookChapter = bookChapters[chapterIndex];
				bookChapter.name = chapterIndex.ToString();
			}
		}


		private void OnChapterFlipped(string previousChapterName, string newChapterName)
		{
			Debug.Log(bookChapterNavigator.NavigatingChapterName);
			string newNavigatingChapterName = bookChapterNavigator.NavigatingChapterName;
			var newBookChapter = bookChapterDic[newNavigatingChapterName];
			int navigatedPageRecord = pageNavigationRecordOnChapter[newNavigatingChapterName];

			bookPageNavigator.RecieveNewChapter(newBookChapter.BookPageObjs, navigatedPageRecord);
		}
		private void OnPageFlipped(List<GameObject> navigatingPages, int navigatingIndex)
		{
			Debug.Log(bookChapterNavigator.NavigatingChapterName);
			pageNavigationRecordOnChapter[bookChapterNavigator.NavigatingChapterName] = navigatingIndex;
		}


		private List<GameObject> GetGameObjList<T>(List<T> originalList) where T : BookChapter
		{
			List<GameObject> gameObjList = new();

			foreach (var element in originalList) gameObjList.Add(element.gameObject);

			return gameObjList;
		}


	}
}