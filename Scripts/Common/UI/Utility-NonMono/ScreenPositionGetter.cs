using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenPositionGetter
{
	static int divideNum = 5;



	public static Vector3 GetScreenPosition(ScreenPosition screenPosition, float camDistance)
	{
		int hCenter = Screen.width / 2,  vCenter = Screen.height / 2;
		int hSpacing = Screen.width / divideNum,  vSpacing = Screen.height / divideNum;

		var xPosition = hCenter + hSpacing * (int)screenPosition.horizontalAlignment;
		var yPosition = vCenter + vSpacing * (int)screenPosition.verticalAlignment;

		return new Vector3(xPosition, yPosition, camDistance);

	}

	public static Vector3 GetScreenPosition(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float camDistance)
	{
		int hCenter = Screen.width / 2,  vCenter = Screen.height / 2;
		int hSpacing = Screen.width / divideNum,  vSpacing = Screen.height / divideNum;

		var xPosition = hCenter + hSpacing * (int)horizontalAlignment;
		var yPosition = vCenter + vSpacing * (int)verticalAlignment;

		return new Vector3(xPosition, yPosition, camDistance);

	}

	public static Vector2 GetScreenPosition(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
	{
		int hCenter = Screen.width / 2,  vCenter = Screen.height / 2;
		int hSpacing = Screen.width / divideNum,  vSpacing = Screen.height / divideNum;

		var xPosition = hCenter + hSpacing * (int)horizontalAlignment;
		var yPosition = vCenter + vSpacing * (int)verticalAlignment;

		return new Vector2(xPosition, yPosition);

	}

	/// <summary> closeness(0~...): 클수록 중심으로 가깝게 Left, Right, Bottom, Upper 배치 됨 </summary>
	/// <param name="closeness">  </param>
	/// <returns></returns>
	public static Vector2 GetScreenPosition(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, int closeness)
	{
		// TODO: 코드 정리
		if (closeness < 0) closeness = 0;

		int hCenter = Screen.width / 2,  vCenter = Screen.height / 2;
		int hSpacing = Screen.width / (2 + closeness),  vSpacing = Screen.height / (2 + closeness);

		var xPosition = hCenter + hSpacing * (int)horizontalAlignment;
		var yPosition = vCenter + vSpacing * (int)verticalAlignment;

		return new Vector2(xPosition, yPosition);

	}



}


public enum HorizontalAlignment
{ Center = 0, Left = -1, Right = 1 }
public enum VerticalAlignment 
{ Center = 0, Bottom = -1, Upper = 1 }

[System.Serializable]
public class ScreenPosition
{
	public HorizontalAlignment horizontalAlignment;
	public VerticalAlignment verticalAlignment;
}


