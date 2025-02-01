using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// Npc E ->
// Notebook Create ->
// OnNotebookReady:InitializeOnNotebookLog ->
// OnNotebookLogInitialized:HoldNotebook ->
// DialogueInfo.OnNpcDialogueStart(npc);
public class NotebookController : MonoBehaviour
{
	// TODO: 추후, place에 따라 hold하는 노트북Info을 다르게 하면 됨
	[SerializeField] UDictionary<PlaceInfo, NotebookInfo> notebookPerNpcPlace;


	#region Notebook UI Objects

	[SerializeField] Transform notebookCanvas;
	[SerializeField] GameObject leftButtonObj;
	[SerializeField] GameObject rightButtonObj;

	[HideInInspector] public Transform NotebookCanvas { get => notebookCanvas; }
	[HideInInspector] public GameObject LeftButtonObj { get => leftButtonObj; }
	[HideInInspector] public GameObject RightButtonObj { get => rightButtonObj; }

	#endregion



	private void Update()
	{
		if (PlayerStatusManager.CurrentInterStatus == InteractionStatus.None)
			if (this.gameObject && this.gameObject.activeSelf) gameObject.SetActive(false);
	}

	public NotebookInfo GetNotebookInfo(PlaceInfo placeInfo) {  return notebookPerNpcPlace[placeInfo];  }



	// 노트북 flip -> notebookHold -> flip -> setactive(panel)
	//                  -> pageMove + setactive(prevPanel.false)  -> flip -> setactive(nextPanel.true)
	//                  -> setactive(panel.false) & flip
	public void FlipNotebook() {  transform.GetComponent<Animator>().Play("Page_ParentAction");  }

}
