using UnityEngine.Events;
using UnityEngine.EventSystems;


public static class EventTriggerLinker
{
	public static void LinkEventTriggerTo<T>(EventTrigger targetEventTrigger, EventTriggerType eventTriggerType, UnityAction<T> action) where T : BaseEventData
	{
		var newEntry = new EventTrigger.Entry { eventID = eventTriggerType };
		newEntry.callback.AddListener((data) => { action((T)data); });
		targetEventTrigger.triggers.Add(newEntry);
	}
	public static void LinkEventTriggerTo<T1, T2>(EventTrigger targetEventTrigger, EventTriggerType eventTriggerType, UnityAction<T1, T2> action, T2 actionParameter) where T1 : BaseEventData
	{
		var newEntry = new EventTrigger.Entry { eventID = eventTriggerType };
		newEntry.callback.AddListener((data) => { action((T1)data, actionParameter); });
		targetEventTrigger.triggers.Add(newEntry);
	}
}