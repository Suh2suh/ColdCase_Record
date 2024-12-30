using UnityEngine;


public abstract class MouseHoverChecker : MonoBehaviour
{
	[Space(15)]
	[SerializeField] protected InteractionStatus mouseHovableStatus;

	private bool isMouseHovering;
	public bool IsMouseHovering
	{
		get => isMouseHovering;
		set
		{
			if(isMouseHovering != value)
			{
				isMouseHovering = value;

				if (isMouseHovering == true)
					OnMouseHover();
				else 
					OnMouseLeave();
			}
		}
	}


	#region Unity Methods

	private void Awake()
	{
		isMouseHovering = false;
	}

	protected virtual void Start()
	{
		// Layer 8: Mouse Interactive Object 
		if (this.gameObject.layer != 8)   this.gameObject.layer = 8;
	}


	#endregion


	protected abstract void OnMouseHover();

	protected abstract void OnMouseLeave();


}
