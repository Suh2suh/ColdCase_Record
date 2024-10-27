using TMPro;
using UnityEngine;

namespace ColdCase.Dialogue.Book.Object
{
	public class BookChapterIndexer : MonoBehaviour
	{
		TextMeshPro textOnIndexer;
		MeshRenderer indexerRenderer;

		#region Initializers

		public void Initialize(string indexerText)
		{
			textOnIndexer = this.transform.GetComponentInChildren<TextMeshPro>();

			ChangeIndexerText(indexerText);
		}

		public void Initialize(Color defaultColor)
		{
			var coloredSection = this.transform.Find("Indexer_Colored");   // change this <indexer_Colored> to free
			indexerRenderer = coloredSection.GetComponent<MeshRenderer>();

			ChangeColorTo(defaultColor);
		}

		public void Initialize(string indexerText, Color defaultColor)
		{
			textOnIndexer = this.transform.GetComponentInChildren<TextMeshPro>();

			var coloredSection = this.transform.Find("Indexer_Colored");   // change this <indexer_Colored> to free
			indexerRenderer = coloredSection.GetComponent<MeshRenderer>();

			ChangeIndexerText(indexerText);
			ChangeColorTo(defaultColor);
		}


		#endregion

		public void ChangeColorTo(Color newColor)
		{
			if(indexerRenderer)   indexerRenderer.material.SetColor("_BaseColor", newColor);
		}

		public void ChangeIndexerText(string newText)
		{
			if(textOnIndexer)   textOnIndexer.text = newText;
		}


	}
}