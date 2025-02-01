using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HotKey
{
	Observe, Obtain, Interact, Talk, Photo, Escape
}


[CreateAssetMenu(fileName = "HotKeyInfo", menuName = "ScriptableObjects/Informations/HotKeyInfo", order = 2)]
public class HotKeyInfo : ScriptableObject
{
	// TODO: key change option �߰� �� ���⼭ �ʱ�ȭ�ϸ� �� ��, ��� �Ʒ��� �ʱ�ȭ�Ǳ� ������ ������ Ű ���ư�
	// https://docs.unity3d.com/Manual/class-InputManager.html -> key conversion ����
	//UDictionary<HotKey, string> hotKeyDic = new()
	//{
	//	{ HotKey.Observe, "e" },
	//	{ HotKey.Obtain, "f" },
	//	{ HotKey.Interact, "e" },
	//	{ HotKey.Talk, "e" },
	//	{ HotKey.Photo, "c" },
	//	{ HotKey.Escape, "escape" }
	//};
	UDictionary<HotKey, KeyCode> hotKeyDic = new()
	{
		{ HotKey.Observe, KeyCode.E },
		{ HotKey.Obtain, KeyCode.F },
		{ HotKey.Interact, KeyCode.E },
		{ HotKey.Talk, KeyCode.E },
		{ HotKey.Photo, KeyCode.C },
		{ HotKey.Escape, KeyCode.Escape }
	};
	//public UDictionary<HotKey, string> HotKeyDic  { get => hotKeyDic; }
	public UDictionary<HotKey, KeyCode> HotKeyDic  { get => hotKeyDic; }


}