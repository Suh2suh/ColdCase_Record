using System.Collections;
using UnityEngine;

public class IF_Drawer : InteractiveFurnitureController
{
	#region Setting Variables
	[SerializeField] private float drawDistance = 0.3f;
	[SerializeField] private bool isOpened = false;

	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip openSFX;
	[SerializeField] private AudioClip closeSFX;

	#endregion

	#region Private Variables
	private Vector3 closedPosition;
	private Vector3 openedPosition;

	private bool isCoroutineOn = false;
	private float drawDuration = 0.5f;

	#endregion


	private void Awake()
	{
		closedPosition = transform.localPosition;
		openedPosition = closedPosition + (Vector3.forward * drawDistance);
	}


	public override void Interact()
	{
		if (!isCoroutineOn)
		{
			audioSource.PlayOneShot(openSFX);
			StartCoroutine(InteractDrawer());
		}
	}

	
	private IEnumerator InteractDrawer()
	{
		isCoroutineOn = true;
		Vector3 startPos = (isOpened ? openedPosition : closedPosition);
		Vector3 goalPos = (isOpened ? closedPosition : openedPosition);

		float time = 0f;
		float lerpT = time / drawDuration;

		while(transform.localPosition != goalPos)
		{
			transform.localPosition = Vector3.Lerp(startPos, goalPos, lerpT);

			time += Time.deltaTime;
			lerpT = time / drawDuration;

			yield return null;
		}

		if (isOpened) CloseDrawer();
		else		  OpenDrawer();

		isCoroutineOn = false;
	}


	private void OpenDrawer()
	{
		transform.localPosition = closedPosition + (Vector3.forward * drawDistance);

		isOpened = true;
	}

	void CloseDrawer()
	{
		transform.localPosition = closedPosition;

		isOpened = false;
	}


}