using System.Collections;
using UnityEngine;

public class IF_Drawer : InteractiveFurnitureController
{
	[SerializeField] float drawDistance = 0.3f;
	[SerializeField] bool isOpened = false;

	[SerializeField] AudioSource audioSource;
	[SerializeField] AudioClip openSFX;
	[SerializeField] AudioClip closeSFX;
	
	Vector3 closedPosition;
	Vector3 openedPosition;

	bool isCoroutineOn = false;
	float drawDuration = 0.5f;


	private void Awake()
	{
		closedPosition = transform.localPosition;
		openedPosition = closedPosition + (Vector3.forward * drawDistance);
	}


	override public void Interact()
	{
		if (!isCoroutineOn)
		{
			audioSource.PlayOneShot(openSFX);
			StartCoroutine(InteractDrawer());
		}
	}

	
	IEnumerator InteractDrawer()
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
		else				  OpenDrawer();
		isCoroutineOn = false;
	}


	void OpenDrawer()
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