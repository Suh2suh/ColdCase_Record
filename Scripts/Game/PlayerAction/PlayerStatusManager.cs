using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InteractionStatus
{
	None, 

	ObservingObject,     // ������Ʈ ���� ��
	Obtaining,      // ������Ʈ ȹ�� ��

	TalkingNpc,          // NPC ��ȭ ��
	TalkingWalkieTalkie,

	Photo,            // ī�޶� ��� ��

	Investigating,  // ���� ����ũ ��ȣ�ۿ� ��
	Inventory,        // �κ��丮 ���� ��
	ObservingPlace,
		//,
	//Interacting      // ���� ��ȣ�ۿ� �� (������ ���ٰų�... ���)
}


public static class PlayerStatusManager
{
	public static System.Action OnInteractionStatusUpdated;


	/// <summary>  ���� ȹ��: �ʵ� / ���� ��, ��ȭ: �ʵ�, ������... �� �����ϱ� ����  </summary>
	static InteractionStatus prevInterStatus = InteractionStatus.None;

	static InteractionStatus currentInterStatus = InteractionStatus.None;


	/// <summary>  ĳ���� ��ȣ�ۿ� ���� ����  </summary>
	public static void SetInterStatus(InteractionStatus interStatus)
	{
		if (currentInterStatus != interStatus)
		{
			prevInterStatus = currentInterStatus;
			currentInterStatus = interStatus;

			OnInteractionStatusUpdated();
			Debug.Log(prevInterStatus + " -> " + currentInterStatus + " / " + PlayerStatusManager.GetCurrentInterStatus());
		}
	}

	public static InteractionStatus GetCurrentInterStatus() { return currentInterStatus; }
	public static InteractionStatus GetPrevInterStatus() { return prevInterStatus; }

}
