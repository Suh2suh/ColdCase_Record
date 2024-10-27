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
	// TODO: key change option 추가 시 여기서 초기화하면 안 됨, 계속 아래로 초기화되기 때문에 설정한 키 날아감
	// https://docs.unity3d.com/Manual/class-InputManager.html -> key conversion 참고
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