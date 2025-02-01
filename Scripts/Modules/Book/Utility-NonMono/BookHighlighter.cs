using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


namespace ColdCase.Dialogue.Book.Utility
{
	public class BookHighlighter
	{
		#region Private Variables
		private TextMeshProUGUI pointingSentence;
		private int pointingCharIndexInSentence = -1;
		private Vector2 SentenceLocaledMousePos = Vector2.zero;

		#endregion

		protected static string highlightStartTag;
		protected static string highlightEndTag;
		protected Dictionary<string, Dictionary<string, string>> markedKeyTextPerSheet;
		public Dictionary<string, Dictionary<string, string>> MarkedKeyTextPerSheet { get => markedKeyTextPerSheet; }


		/// <summary> 
		/// <b>*Must* use this method on update,</b> for highlighting texts 
		/// </summary>
		public void ManageTextHighlightOnUpdate()
		{
			if (pointingSentence == null) return;

			if (Input.GetMouseButton(0))
			{
				SentenceLocaledMousePos = GetUILocaledMousePos(pointingSentence.transform);
				pointingCharIndexInSentence = GetRealIndexOfPointingChar(pointingSentence.textInfo, SentenceLocaledMousePos);

				HighlightCharacter(pointingSentence, pointingCharIndexInSentence);
			}
			else
			if (Input.GetMouseButton(1))
			{
				SentenceLocaledMousePos = GetUILocaledMousePos(pointingSentence.transform);
				pointingCharIndexInSentence = GetRealIndexOfPointingChar(pointingSentence.textInfo, SentenceLocaledMousePos);

				DeHighlightCharacter(pointingSentence, pointingCharIndexInSentence);
			}
		}


		/// <summary>  
		/// If you want to save highlighted record, update it with book by using markedKeyTextPerSheet  
		/// </summary>
		public void Postprocess(System.Action callback)
		{
			pointingSentence = null;

			callback();
		}


		#region [Action]: Highlight / DeHilight

		private int prevCharIndex = -1;
		private void HighlightCharacter(TextMeshProUGUI sentence, int targetCharIndex)
		{
			if (targetCharIndex < 0 || prevCharIndex == targetCharIndex ||
				IsCharacterHighlighted(sentence.text, targetCharIndex)) return;

			string front = sentence.text.Substring(0, targetCharIndex) + highlightStartTag;
			char markTarget = sentence.text[targetCharIndex];
			string back = highlightEndTag + sentence.text.Substring(targetCharIndex + 1);


			string markedText = front + markTarget + back;
			sentence.text = markedText;
			
			// save highlighted text
			string sheetOfSentence = sentence.GetComponent<DialogueSentenceInfo>().bookSentence.sheetName;
			AddRecordHighlightedText(sheetOfSentence, sentence.transform.name);

			prevCharIndex = targetCharIndex;
		}

		private void DeHighlightCharacter(TextMeshProUGUI sentence, int targetCharIndex)
		{
			if (targetCharIndex < 0 || !IsCharacterHighlighted(sentence.text, targetCharIndex)) return;

			string front = sentence.text.Substring(0, targetCharIndex - highlightStartTag.Length);
			char unMarkTarget = sentence.text[targetCharIndex];
			string back = sentence.text.Substring(targetCharIndex + highlightEndTag.Length + 1);

			string unMarkedText = front + unMarkTarget + back;
			sentence.text = unMarkedText;

			string sheetOfSentence = sentence.GetComponent<DialogueSentenceInfo>().bookSentence.sheetName;
			AddRecordHighlightedText(sheetOfSentence, sentence.transform.name);
		}


		private Vector2 GetUILocaledMousePos(Transform targetUI)
		{
			var rectTransform = targetUI.GetComponent<RectTransform>();
			var mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, Camera.main, out var localMousePos);

			return localMousePos;
		}


		private int GetRealIndexOfPointingChar(TMP_TextInfo textInfo, Vector2 localMousePos)
		{
			TMP_LineInfo pointingLineInfo;

			int pointingLineIndex = GetIndexOfPointingLine(textInfo, localMousePos);

			if (pointingLineIndex >= 0)
				pointingLineInfo = textInfo.lineInfo[pointingLineIndex];
			else
				return -1;

			int pointingCharIndex = GetIndexOfPointingChar(textInfo, pointingLineInfo, localMousePos);
			//Debug.Log(pointingLineInfo.ToString() + " / " + pointingCharIndex);

			return pointingCharIndex;
		}

		private int GetIndexOfPointingLine(TMP_TextInfo textInfo, Vector2 position)
		{
			int pointingLineIndex = -1;

			for (int lineIndex = 0; lineIndex < textInfo.lineCount; lineIndex++)
			{
				var line = textInfo.lineInfo[lineIndex];

				if (position.x >= textInfo.characterInfo[line.firstVisibleCharacterIndex].bottomLeft.x && position.x <= textInfo.characterInfo[line.lastVisibleCharacterIndex].bottomRight.x)
					if (position.y >= line.descender && position.y <= line.ascender)
						return lineIndex;
			}

			return pointingLineIndex;
		}

		private int GetIndexOfPointingChar(TMP_TextInfo textInfo, TMP_LineInfo lineInfo, Vector2 position)
		{
			int pointingCharIndex = -1;

			for (int charIndex = lineInfo.firstCharacterIndex; charIndex < lineInfo.firstCharacterIndex + lineInfo.characterCount; charIndex++)
			{
				var visibleCharacterInfo = textInfo.characterInfo[charIndex];

				if (position.x >= visibleCharacterInfo.bottomLeft.x && position.x <= visibleCharacterInfo.bottomRight.x)
				{
					return visibleCharacterInfo.index;
				}
			}

			return pointingCharIndex;
		}


		private bool IsCharacterHighlighted(string fullText, int realCharIndex)
		{
			if (realCharIndex - highlightStartTag.Length < 0) return false;

			bool isMarkTagFront = fullText.Substring(realCharIndex - highlightStartTag.Length, highlightStartTag.Length) == highlightStartTag;
			return isMarkTagFront;
		}


		#endregion

		#region [Action]: Save Highlight Record

		private void AddRecordHighlightedText(string sheetName, string key)
		{
			if (!markedKeyTextPerSheet.ContainsKey(sheetName))
				markedKeyTextPerSheet[sheetName] = new Dictionary<string, string> { { key, "" } };
			else
				markedKeyTextPerSheet[sheetName][key] = "";
		}

		public void RemoveKeyOfHighlightTextRecord(string sheetName, string key)
		{
			if (markedKeyTextPerSheet.ContainsKey(sheetName))
				if (markedKeyTextPerSheet[sheetName].ContainsKey(key))
					markedKeyTextPerSheet[sheetName].Remove(key);
		}


		#endregion


		#region HighlightableText Linker
		/// <summary> Put Any Text Transform to make it highlightable </summary>
		public void LinkHighlightRecieverTo(Transform textTransform, string sheetName)
		{
			var textEventTrigger = textTransform.GetComponent<EventTrigger>() ?? textTransform.gameObject.AddComponent<EventTrigger>();
			EventTriggerLinker.LinkEventTriggerTo<PointerEventData, Transform>(textEventTrigger, EventTriggerType.PointerEnter, EnableHighlightOnPointerEnter, textTransform);
			EventTriggerLinker.LinkEventTriggerTo<PointerEventData, Transform>(textEventTrigger, EventTriggerType.PointerExit, DisableHighlightOnPointerExit, textTransform);

			var sentenceInfo = textTransform.GetComponent<DialogueSentenceInfo>() ?? textTransform.gameObject.AddComponent<DialogueSentenceInfo>();
			sentenceInfo.bookSentence.sheetName = sheetName;
			sentenceInfo.bookSentence.keyCode = textTransform.name;
		}

		// 링크 -> 하이라이트 가능하게 하는 Function
		void EnableHighlightOnPointerEnter(PointerEventData data, Transform textTransform)
		{
			pointingSentence = textTransform.GetComponent<TextMeshProUGUI>();
		}

		void DisableHighlightOnPointerExit(PointerEventData data, Transform textTransform)
		{
			pointingSentence = null;

			var sentenceInfo = textTransform.GetComponent<DialogueSentenceInfo>();
			if (markedKeyTextPerSheet.ContainsKey(sentenceInfo.bookSentence.sheetName) && markedKeyTextPerSheet[sentenceInfo.bookSentence.sheetName].ContainsKey(textTransform.name))
				markedKeyTextPerSheet[sentenceInfo.bookSentence.sheetName][textTransform.name] = textTransform.GetComponent<TextMeshProUGUI>().text;
		}




		#endregion


	}
}
