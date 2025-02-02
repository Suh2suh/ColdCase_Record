using UnityEngine;


public static class PlayerStatusManager
{
	public static System.Action OnInteractionStatusUpdated;


	/// <summary>  ���� ȹ��: �ʵ� / ���� ��, ��ȭ: �ʵ�, ������... �� �����ϱ� ����  </summary>
	private static InteractionStatus prevInterStatus = InteractionStatus.None;
	private static InteractionStatus currentInterStatus = InteractionStatus.None;

	public static InteractionStatus PrevInterStatus => prevInterStatus;
	public static InteractionStatus CurrentInterStatus => currentInterStatus;


	/// <summary>  ĳ���� ��ȣ�ۿ� ���� ����  </summary>
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
