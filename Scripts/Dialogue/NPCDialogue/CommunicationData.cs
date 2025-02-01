

[System.Serializable]
public struct CommunicationData
{
	public CharacterCode characterCode;

	public Face faceExpression;
	public Pronounce pronounce;
	public UDictionary<Language, string> dialogueByLanguage;
}


public enum CharacterCode
{
	None, Npc, Player
}

public enum Language
{
	Korean = 0, English = 1, Japanese = 2, Chinese
}

public enum Face
{
	None, Frown, Smile, Tearful
}

public enum Pronounce
{
	None, a, e, i, o, u
}
