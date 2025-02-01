using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HotKeyChecker : MonoBehaviour
{
    [HideInInspector, SerializeField] HotKeyInfo hotKeyInfo;

    public static Dictionary<HotKey, bool> isKeyPressed;


	private void Awake()
	{
        isKeyPressed = new();

        foreach(var hotKey in hotKeyInfo.HotKeyDic.Keys)
		{
            isKeyPressed.Add(hotKey, false);
            //Debug.Log(hotKey);
		}

	}


	void Update()
    {

        foreach(var hotKeyPair in isKeyPressed.ToList())
		{
            UpdatePressedKey(hotKeyPair.Key);
        }

    }


    void UpdatePressedKey(HotKey hotKey)
	{
        isKeyPressed[hotKey] = Input.GetKeyDown(hotKeyInfo.HotKeyDic[hotKey]);

    }


}
