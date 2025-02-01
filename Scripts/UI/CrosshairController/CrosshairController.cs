using System.Collections;
using UnityEngine;
using UnityEngine.UI;


// CrosshairParent
public class CrosshairController : MonoBehaviour
{
	[SerializeField, Space(15)]
    protected Image ImageCrossHair;
    protected Color defaultCHColor;
    protected Sprite defaultCHSprite; 


	#region UnIty Methods
	protected virtual void Awake()
	{
        defaultCHColor = ImageCrossHair.color;
        defaultCHSprite = ImageCrossHair.sprite;
    }


    #endregion



    #region Color 

    public void ChangeCHColor(Color color)
    {
        if (ImageCrossHair.color != color)
            ImageCrossHair.color = new Color(color.r, color.g, color.b, defaultCHColor.a);
    }

    public void RevertCHColor()
    {
        if (ImageCrossHair.color != defaultCHColor)
            ImageCrossHair.color = defaultCHColor;
    }


    #endregion


    #region Activation Control

    public virtual void ActivateCH()
    {
        ActivateCrossHairImage(true);

        //if (isCHKeyHintValid) ActivateKeyHintText(true);
    }

    public virtual void DeActivateCH()
    {
        ActivateCrossHairImage(false);

        //if (isCHKeyHintValid) ActivateKeyHintText(false);
    }

    protected void ActivateCrossHairImage(bool activeStatus)
    {
        if (ImageCrossHair.gameObject.activeSelf != activeStatus) ImageCrossHair.gameObject.SetActive(activeStatus);
    }


	#endregion

}