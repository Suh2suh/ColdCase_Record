using System.Collections.Generic;
using UnityEngine;


namespace ColdCase.Dialogue.Book.Object
{
	public class BookChapter : MonoBehaviour
	{
		[SerializeField] List<BookPage> bookPages;
		[HideInInspector] public List<BookPage> BookPages { get => bookPages; }
		
		List<GameObject> bookPageObjs;
		[HideInInspector] public List<GameObject> BookPageObjs { get => bookPageObjs; }


		private void Awake()
		{
			bookPageObjs = new();
			foreach (var bookPage in BookPages)
			{
				if(bookPage != null)
					bookPageObjs.Add(bookPage.gameObject);
			}
		}
	}
}

