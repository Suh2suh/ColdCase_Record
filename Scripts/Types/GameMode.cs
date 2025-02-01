

public enum GameMode
{
	/// <summary>
	/// Game time restarts automatically (DeltaTime(1))
	/// </summary>
	InGame,

	/// <summary>
	/// When player is using outgame UI (e.g.) Setting, Reward...
	/// <para> Game time stops automatically (DeltaTime(0)) </para>
	/// </summary>
	OutGame,

	/// <summary>
	/// When player is watching game video / pictures
	/// <para> Game time stops automatically (DeltaTime(0)) </para>
	/// </summary>
	Media
}