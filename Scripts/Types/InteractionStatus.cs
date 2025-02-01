

public enum InteractionStatus
{
	None,

	ObservingObject,     // 오브젝트 관찰 중
	Obtaining,      // 오브젝트 획득 중

	TalkingNpc,          // NPC 대화 중
	TalkingWalkieTalkie,

	Photo,            // 카메라 드는 중

	Investigating,  // 형사 데스크 상호작용 중
	Inventory,        // 인벤토리 보는 중
	ObservingPlace,
	//,
	//Interacting      // 물건 상호작용 중 (서랍을 연다거나... 등등)
}
