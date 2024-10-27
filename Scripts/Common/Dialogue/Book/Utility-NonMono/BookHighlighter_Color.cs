using System.Collections;
using UnityEngine;

namespace ColdCase.Dialogue.Book.Utility
{
	public class BookHighlighter_Color: BookHighlighter
	{

		public BookHighlighter_Color(Color highlightColor)
		{
			string markTagColor = "#" + ColorUtility.ToHtmlStringRGBA(highlightColor);
			highlightStartTag = "<color=" + markTagColor + ">";
			highlightEndTag = "</color>";

			markedKeyTextPerSheet = new();
		}


	}

}