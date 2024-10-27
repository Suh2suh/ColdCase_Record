using System.Collections.Generic;
using UnityEngine;



namespace ColdCase.Dialogue.Book.Utility
{
	public class BookChapterNavigator
	{
		public bool isBookChapterNavigatable = true;


		/// <summary> must match with BookChapterIndexerCreator.IndexerLayer </summary>
		static readonly int indexerLayer = 1 << 10;


		bool isHoveringChapterIndexer;

		Transform lastHoveredIndexer;
		Transform newHoveredIndexer;


		Dictionary<string, GameObject> bookChapterDic;

		string navigatingChapterName;
		public string NavigatingChapterName { get => navigatingChapterName; }


		public System.Action<string, string> OnChapterFlipped;
		public System.Action OnIndexerPointerEnter;
		public System.Action OnIndexerPointerExit;


		public BookChapterNavigator(List<GameObject> bookChapters, int initialChapterIndex)
		{
			bookChapterDic = new();

			for (int chapterIndex = 0; chapterIndex < bookChapters.Count; chapterIndex++)
			{
				GameObject bookChapter = bookChapters[chapterIndex];
				bookChapterDic[bookChapter.name] = bookChapter;

				if (chapterIndex != initialChapterIndex) bookChapter.SetActive(false);
				else
				{
					bookChapter.SetActive(true);
					navigatingChapterName = bookChapter.name;
				}
			}
		}
		public BookChapterNavigator(List<GameObject> bookChapters, string initialChapterName)
		{
			bookChapterDic = new();
			for (int chapterIndex = 0; chapterIndex < bookChapters.Count; chapterIndex++)
			{
				GameObject bookChapter = bookChapters[chapterIndex];

				bookChapterDic[bookChapter.name] = bookChapter;

				if (bookChapter.name != initialChapterName) bookChapter.SetActive(false);
				else
				{
					bookChapter.SetActive(true);
					navigatingChapterName = bookChapter.name;
				}
			}
		}


		public void ManageChapterNavigationOnUpdate()
		{
			if (!isBookChapterNavigatable) return;

			if (isHoveringChapterIndexer && Input.GetMouseButtonDown(0))
			{
				if (!bookChapterDic.ContainsKey(newHoveredIndexer.name))
					return;
					

				NavigateNpcChapterTo(newHoveredIndexer.name);
			}
		}


		// indexer check는 한 번에 하나만 하기 때문에, 굳이 여러 인스턴스 함수가 있을 필요 X
		/// <summary> <b>*Must*0 use this method on update,</b> for indexer mouse check </summary>
		public void CheckPointingIndexerOnFixedUpdate()
		{
			newHoveredIndexer = ObjectSorter.GetRayhitOnMouse(indexerLayer, 50f);

			// On Pointer Enter Over Indexer
			if (newHoveredIndexer != null && !isHoveringChapterIndexer)
			{
				isHoveringChapterIndexer = true;

				if(newHoveredIndexer.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
					mouseHoverChecker.IsMouseHovering = isHoveringChapterIndexer;

				lastHoveredIndexer = newHoveredIndexer;
				if(OnIndexerPointerEnter != null) OnIndexerPointerEnter();
			}
			// On Pointer Exit From Indexer
			else
			if (newHoveredIndexer == null && isHoveringChapterIndexer)
			{
				isHoveringChapterIndexer = false;

				if (lastHoveredIndexer.TryGetComponent<MouseHoverChecker>(out var mouseHoverChecker))
					mouseHoverChecker.IsMouseHovering = isHoveringChapterIndexer;

				lastHoveredIndexer = null;
				if(OnIndexerPointerExit != null) OnIndexerPointerExit();
			}
		}

		public void NavigateNpcChapterTo(string newChapterName)
		{
			// Previous Navigating Npc
			string previousChapterName = navigatingChapterName;
			bookChapterDic[previousChapterName].SetActive(false);


			// New Navigating Npc
			navigatingChapterName = newChapterName;
			bookChapterDic[newChapterName].SetActive(true);

			if(OnChapterFlipped != null) OnChapterFlipped(previousChapterName, newChapterName);
		}
	}
}
