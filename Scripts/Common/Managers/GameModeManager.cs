using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameMode
{
	Game,   // Game Mode -> DeltaTime(1)

	Setting,  // 업적 등 Uitility 기능 -> DeltaTime(0)

	Media    // Video 시청 -> 기능 전부 저장 -> DeltaTime(0)
}



public static class GameModeManager
{
	
	public static System.Action<GameMode> OnGameModeUpdated;


    static GameMode currentGameMode = GameMode.Game;
    public static GameMode CurrentGameMode { get => currentGameMode; set => currentGameMode = value; }



	public static GameMode GetCurrentGameMode()
	{
        return currentGameMode;
	}

    public static void SetGameMode(GameMode newGameMode)
    {
        // Debug.Log(currentGameMode + " ? " + newGameMode);
        if (currentGameMode != newGameMode)
        {
            currentGameMode = newGameMode;
            if(OnGameModeUpdated != null) OnGameModeUpdated(newGameMode);

            if (currentGameMode == GameMode.Game) ResumeGame();
            else PauseGame();
        }
    }



    /// <summary>
    /// If there is some obj/anim that you don't want to stop during setting panel is on, Make it "Unscaled Time"
    /// </summary>
    public static void PauseGame()
    {
        Time.timeScale = 0f;

        //Debug.Log(GetCurrentGameMode() + " Pause");
    }
    public static void ResumeGame()
    {
        Time.timeScale = 1f;

        //Debug.Log(GetCurrentGameMode() + " Resume");
    }


}
