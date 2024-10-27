using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InteractionStatus
{
	None, 

	ObservingObject,     // 오브젝트 관찰 중
	Obtaining,      // 오브젝트 획득 중

	TalkingNpc,          // NPC 대화 중
	TalkingWalkieTalkie,

	Photo,            // 카메라 드는 중

	Investigating,  // 형사 데스크 상호작용 중
	Inventory,        // 인벤토리 보는 중
	ObservingPlace,
		//,
	//Interacting      // 물건 상호작용 중 (서랍을 연다거나... 등등)
}


public static class PlayerStatusManager
{
	public static System.Action OnInteractionStatusUpdated;


	/// <summary>  증거 획득: 필드 / 관찰 중, 대화: 필드, 취조실... 등 구분하기 위해  </summary>
	static InteractionStatus prevInterStatus = InteractionStatus.None;

	static InteractionStatus currentInterStatus = InteractionStatus.None;


	/// <summary>  캐릭터 상호작용 상태 설정  </summary>
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
