

public static class GameModeManager
{

    public static System.Action<GameMode> OnGameModeUpdated;

    private static GameMode currentGameMode = GameMode.InGame;
    public static GameMode CurrentGameMode => currentGameMode;


	public static void SetGameMode(GameMode newGameMode)
    {
        // Debug.Log(currentGameMode + " ? " + newGameMode);
        if (currentGameMode != newGameMode)
        {
            currentGameMode = newGameMode;
            OnGameModeUpdated?.Invoke(newGameMode);

			if (currentGameMode == GameMode.InGame)
                GameTimeManager.ResumeGame();
            else
				GameTimeManager.PauseGame();
        }
    }


}
