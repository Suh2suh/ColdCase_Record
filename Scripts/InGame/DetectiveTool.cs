using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DetectiveTool", menuName = "ScriptableObjects/Objects/DetectiveTool", order = 1)]
public class DetectiveTool : ScriptableObject
{
	public bool isTutorialed;
	public NarrationData_Clip tutorialNarrationData;
	// key & audioClip -> audio Clip�� ������??

	public void SetIsTutorialed(bool isTutorialed)
	{
		this.isTutorialed = isTutorialed;

#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif

	}


	// tutorial mode & ���� tool ���� -> emit blink, �ٸ� ���� Ŭ�� �Ұ��ϰ� �� ��
	// emit blink -> if(detective mode & not finished & not hover) ���� object type ��� blink
	// �ٸ� �� Ŭ�� �Ұ� -> tutorial mode mouse check: if(���� tool ����) hover on
}
