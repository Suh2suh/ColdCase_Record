using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DetectiveTool", menuName = "ScriptableObjects/Objects/DetectiveTool", order = 1)]
public class DetectiveTool : ScriptableObject
{
	public bool isTutorialed;
	public NarrationData_Clip tutorialNarrationData;
	// key & audioClip -> audio Clip이 여러개??

	public void SetIsTutorialed(bool isTutorialed)
	{
		this.isTutorialed = isTutorialed;

#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif

	}


	// tutorial mode & 현재 tool 차례 -> emit blink, 다른 것은 클릭 불가하게 할 것
	// emit blink -> if(detective mode & not finished & not hover) 현재 object type 계속 blink
	// 다른 거 클릭 불가 -> tutorial mode mouse check: if(현재 tool 차례) hover on
}
