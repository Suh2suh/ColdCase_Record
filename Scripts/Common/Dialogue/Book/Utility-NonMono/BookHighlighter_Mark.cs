using System.Collections;
using UnityEngine;

namespace ColdCase.Dialogue.Book.Utility
{
	public class BookHighlighter_Mark : BookHighlighter
	{

		public BookHighlighter_Mark(Color highlightColor, Vector4 highlightPadding)
		{
			string markTagColor = "#" + ColorUtility.ToHtmlStringRGBA(highlightColor);
			string markTagPadding = "padding=\"" + highlightPadding[0] + "," + highlightPadding[1] + "," + highlightPadding[2] + "," + highlightPadding[3] + "\"";

			highlightStartTag = "<mark=" + markTagColor + " " + markTagPadding + ">";
			highlightEndTag = "</mark>";

			markedKeyTextPerSheet = new();
		}


	}

}