using UnityEngine;


public static class PlayerStatusManager
{
	public static System.Action OnInteractionStatusUpdated;


	/// <summary>  증거 획득: 필드 / 관찰 중, 대화: 필드, 취조실... 등 구분하기 위해  </summary>
	private static InteractionStatus prevInterStatus = InteractionStatus.None;
	private static InteractionStatus currentInterStatus = InteractionStatus.None;

	public static InteractionStatus PrevInterStatus => prevInterStatus;
	public static InteractionStatus CurrentInterStatus => currentInterStatus;


	/// <summary>  캐릭터 상호작용 상태 설정  </summary>
	public static void SetInterStatus(InteractionStatus interStatus)
	{
		if (currentInterStatus != interStatus)
		{
			prevInterStatus = currentInterStatus;
			currentInterStatus = interStatus;

			OnInteractionStatusUpdated();
			Debug.Log(prevInterStatus + " -> " + currentInterStatus);
		}
	}


}
