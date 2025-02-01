using UnityEngine;


public static class GameTimeManager  //TODO: [250201] 이름 고민
{

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


	// POSSIBLE: [250201] 필요 시 이곳에, Unitask기반 pause/resume 추가

}