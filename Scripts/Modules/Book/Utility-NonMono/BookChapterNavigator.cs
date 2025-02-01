using System.Collections.Generic;
using UnityEngine;



namespace ColdCase.Dialogue.Book.Utility
{
	public class BookChapterNavigator
	{
		#region Private Variables

		/// <summary> must match with BookChapterIndexerCreator.IndexerLayer </summary>
		private static readonly int indexerLayer = 1 << 10;


		private bool isHoveringChapterIndexer;

		private Transform lastHoveredIndexer;
		private Transform newHoveredIndexer;


		private Dictionary<string, GameObject> bookChapterDic;

		private string navigatingChapterName;


		#endregion

		public bool isBookChapterNavigatable = true;
		public string NavigatingChapterName { get => navigatingChapterName; }

		public System.Action<string, string> OnChapterFlipped;
		public System.Action OnIndexerPointerEnter;
		public System.Action OnIndexerPointerExit;


		#region Initializers

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


		#endregion


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


		/// <summary> 
		/// <b>*Must* use this method on update,</b> for indexer mouse check
		/// </summary>
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
