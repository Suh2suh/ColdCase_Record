using ColdCase.Dialogue.Book.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ColdCase.Dialogue.Book.Utility
{

	// Role 1. 타겟 버튼에 페이지 넘김 기능을 부여한다
	//      2. 페이지를 넘긴다
	public class BookPageNavigator
	{
		#region Setting Variables
		public bool isBookPageNavigatable = true;

		#endregion

		#region Private Variables
		private GameObject leftPageButton;
		private GameObject rightPageButton;

		private List<GameObject> bookPages;

		private int navigatingPageIndex;

		#endregion

		public int NavigatingPageIndex
		{
			get => navigatingPageIndex;
			set
			{
				// 외부에서 업데이트되었을 경우 (Ex. NpcDialogue용 Notebook에 새 페이지가 Add되었을 때)
				navigatingPageIndex = value;
				ManagePageButtonActivation();
			}
		}

		public System.Action<List<GameObject>, int> OnPageFlipped;


		#region Initializers

		public BookPageNavigator(GameObject leftPageButton, GameObject rightPageButton)
		{
			LinkPageNavigatorTo(leftPageButton, rightPageButton);
		}
		public BookPageNavigator(GameObject leftPageButton, GameObject rightPageButton, List<GameObject> bookPages, int InitialnavigatingPageIndex)
		{
			LinkPageNavigatorTo(leftPageButton, rightPageButton);

			this.bookPages = bookPages;
			if (bookPages != null) ActivateBookPage(InitialnavigatingPageIndex);

			navigatingPageIndex = InitialnavigatingPageIndex;
			if (bookPages != null) ManagePageButtonActivation();
		}
		public BookPageNavigator(GameObject leftPageButton, GameObject rightPageButton, List<BookPage> bookPages, int InitialnavigatingPageIndex)
		{
			LinkPageNavigatorTo(leftPageButton, rightPageButton);

			this.bookPages = new();
			foreach (var page in bookPages)
			{
				if(page != null)
					this.bookPages.Add(page.gameObject);
			}
			if (bookPages != null) ActivateBookPage(InitialnavigatingPageIndex);

			navigatingPageIndex = InitialnavigatingPageIndex;
			if (bookPages != null) ManagePageButtonActivation();
		}
		public BookPageNavigator(GameObject leftPageButton, GameObject rightPageButton, BookChapter chapter, int InitialnavigatingPageIndex)
		{
			LinkPageNavigatorTo(leftPageButton, rightPageButton);

			this.bookPages = new();
			foreach (var page in chapter.BookPages)
			{
				if(page != null)
					this.bookPages.Add(page.gameObject);
			}
			if (chapter.BookPages != null) ActivateBookPage(InitialnavigatingPageIndex);

			navigatingPageIndex = InitialnavigatingPageIndex;
			if (chapter.BookPages != null) ManagePageButtonActivation();
		}


		#endregion


		public void SetPageNavigationableStatus(bool isNavigatable)
		{
			if (isBookPageNavigatable != isNavigatable) isBookPageNavigatable = isNavigatable;
		}


		#region [Action]: Page Navigation

		public void NavigateToPage(int newNavigatingPageIndex)
		{
			if (!isBookPageNavigatable) return;

			var navigatiedPageIndex = navigatingPageIndex;

			bookPages[navigatiedPageIndex].SetActive(false);
			bookPages[newNavigatingPageIndex].SetActive(true);

			navigatingPageIndex = newNavigatingPageIndex;

			ManagePageButtonActivation();
		}

		private void NavigateToLeftPage()
		{
			if (!isBookPageNavigatable) return;


			// previous Navigation Index
			var navigatiedPageIndex = navigatingPageIndex;

			bookPages[navigatiedPageIndex].SetActive(false);
			if (navigatiedPageIndex == bookPages.Count - 1) rightPageButton.SetActive(true);


			// New Navigation Index
			var navigatingPanelIndex = navigatiedPageIndex - 1;

			bookPages[navigatingPanelIndex].SetActive(true);
			if (navigatingPanelIndex == 0) leftPageButton.SetActive(false);

			navigatingPageIndex = navigatingPanelIndex;
			if (OnPageFlipped != null) OnPageFlipped(bookPages, navigatingPageIndex);
		}

		private void NavigateToRightPage()
		{
			if (!isBookPageNavigatable) return;


			// previous Navigation Index
			var navigatiedPageIndex = navigatingPageIndex;

			bookPages[navigatiedPageIndex].SetActive(false);
			if (navigatiedPageIndex == 0) leftPageButton.SetActive(true);


			// New Navigation Index
			var navigatingPanelIndex = navigatiedPageIndex + 1;

			bookPages[navigatingPanelIndex].SetActive(true);
			if (navigatingPanelIndex == bookPages.Count - 1) rightPageButton.SetActive(false);

			navigatingPageIndex = navigatingPanelIndex;
			if (OnPageFlipped != null) OnPageFlipped(bookPages, navigatingPageIndex);
		}


		#endregion


		#region [Action]: Activation Control

		private void ManagePageButtonActivation()
		{
			if (bookPages.Count == 0 ||
				navigatingPageIndex == 0)
				DeActivatePageButton(0);
			else
				ActivatePageButton(0);

			if (bookPages.Count == 0
				|| navigatingPageIndex == bookPages.Count - 1)
				DeActivatePageButton(1);
			else
				ActivatePageButton(1);

			//Debug.Log("manage");
		}


		public void ActivatePageButton(int button)
		{
			if (button == 0) leftPageButton.SetActive(true);
			else rightPageButton.SetActive(true);

			//Debug.Log("activate :" + button);
		}

		private void DeActivatePageButton(int button)
		{
			if (button == 0) leftPageButton.SetActive(false);
			else rightPageButton.SetActive(false);

			//Debug.Log("deactivate :" + button);
		}


		private void ActivateBookPage(int activateIndex)
		{
			for (int index = 0; index < bookPages.Count; index++)
			{
				var page = bookPages[index];
				if (index == activateIndex) page.SetActive(true);
				else page.SetActive(false);
			}
		}


		#endregion


		public void RecieveNewChapter(List<GameObject> bookPages, int InitialnavigatingPageIndex)
		{
			this.bookPages = bookPages;
			ActivateBookPage(InitialnavigatingPageIndex);

			NavigatingPageIndex = InitialnavigatingPageIndex;

			OnPageFlipped(bookPages, InitialnavigatingPageIndex);
		}


		#region Navigation Button Linker

		void LinkPageNavigatorTo(GameObject leftPageButton, GameObject rightPageButton)
		{
			if (this.leftPageButton == leftPageButton && this.rightPageButton == rightPageButton) return;

			if (this.leftPageButton != null) this.leftPageButton.GetComponent<Button>().onClick.RemoveListener(NavigateToLeftPage);
			if (this.rightPageButton != null) this.rightPageButton.GetComponent<Button>().onClick.RemoveListener(NavigateToLeftPage);

			leftPageButton.GetComponent<Button>().onClick.AddListener(NavigateToLeftPage);
			rightPageButton.GetComponent<Button>().onClick.AddListener(NavigateToRightPage);

			this.leftPageButton = leftPageButton;
			this.rightPageButton = rightPageButton;
		}



		#endregion


	}
}
