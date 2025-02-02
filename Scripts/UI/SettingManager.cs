using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Rendering.HighDefinition;
using System;


public class SettingManager : MonoBehaviour
{
    [SerializeField] private SceneInfo sceneInfo;
    [SerializeField] private DialogueInfo dialogueInfo;

    public static Action OnLanguageChanged;

	#region Unity Methods

	private void Awake()
    {
        #region Awake - Manage Setting Panel Activation

        var childCount = this.transform.childCount;
        for (int i = 0; i < childCount; i++) childs.Add(this.transform.GetChild(i).gameObject);

        #endregion

        #region Awake - Setting Data: Initilaization, Load & Save

        statAudioMixerObj = audioMixerObj;
        statDialogueInfo = dialogueInfo;


        InitializeItemList();

        if (DataManager.isSettingDataExists)
            LoadSettingData();

        else
            InitializeSettingData();


        #endregion

    }

	private void Start()
	{
		#region Active/DeActive objs

        if (sceneInfo.CurrentSceneType == SceneType.Lobby)
        {
            foreach (GameObject child in childs)
            {
				if (!child.activeSelf)
					child.SetActive(true);
			}
        }
        else
		{
            foreach (GameObject child in childs)
            {
				if (child.activeSelf)
					child.SetActive(false);
			}

            if (sceneInfo.CurrentSceneType == SceneType.Game)
            {
                GameObject Player = GameObject.Find("Player");
                playerCanvas = Player.GetComponentInChildren<Canvas>().gameObject;

                //isGamePaused = false;
            }
        }
		#endregion

		if (!gameObject.activeSelf)
                gameObject.SetActive(true);


        #region Setting Methods - Audio & Languages
        UpdateAllVolumes(); // Audio Mixer가 Setting Manager보다 나중에 초기화됨

        #endregion

    }


	private void Update()
    {
        if(sceneInfo.CurrentSceneType == SceneType.Game)
            ManageInGameSettingPanel();
	}

	private void OnDisable()
	{
        SaveSettingData();
    }


	#endregion


	// Proto, Will be Edited
	#region Setting Data: Initilaization, Load & Save

	private Dictionary<string, string> prevSettingDic = new();
    private Dictionary<string, ItemController> ItemDic = new();

	private void InitializeItemList()
	{
        List<ItemController> itemControllers = new();
		GetComponentsInChildren(true, itemControllers);

        foreach (var item in itemControllers)
        {
            item.InitializeItemInfo();

            //ItemDic.Add(item.SettingKey, item);
            if(item.SettingKey != null && item.SettingKey != "")
                ItemDic[item.SettingKey] = item;
        }

        //Debug.Log("Successfully Initialized Item List");
    }

    private void InitializeSettingData()
	{
        foreach (var item in ItemDic)
        {
            prevSettingDic.Add(item.Key, item.Value.SettingValue);
            //Debug.Log(item.Key + ": " + item.Value);
        }
        DataManager.WriteData(DataManager.settingDataPath, JsonConvert.SerializeObject(prevSettingDic));

        //Debug.LogDebug.Log("Successfully Initialized Setting Data");
        Debug.Log("Setting Data None");
    }


	private void LoadSettingData()
	{
        prevSettingDic = JsonConvert.DeserializeObject<Dictionary<string, string>>( DataManager.ReadData(DataManager.settingDataPath) );

        //Debug.Log("Successfully Loaded Setting Data: ");

        foreach(var itemSet in ItemDic)
		{
            var itemKey = itemSet.Key;
            var item = itemSet.Value;

            if (prevSettingDic.ContainsKey(itemKey))
			{
                // 기존 세팅이라면, record 세팅 후 setting value 업데이트
                item.UpdateUIValue(prevSettingDic[itemKey]);
                item.UpdateItemValue();

                // Debug.Log("Original Setting: " + itemKey + " / " + item.SettingValue);

            } else
			{
                prevSettingDic.Add(itemKey, item.SettingValue);

                //Debug.Log("Added Setting: " + itemKey + " / " + item.SettingValue);
			}

            // Apply Previous Setting
            ExecuteSetting(itemKey, prevSettingDic[itemKey]);
        }

        //Debug.Log("Setting Data Exists");
    }

	/// <summary>
	/// Called when the player pressed esc on the setting panel == close the setting panel.
	/// CONSIDER: Save should be when exit game / exit the scene? OR close the panel like now?
	/// </summary>
	private void SaveSettingData()
    {
        DataManager.WriteData(DataManager.settingDataPath, JsonConvert.SerializeObject(prevSettingDic));
    }


    #endregion


    #region Apply Setting

    public void ProcessSetting(ItemController newSet)
    {
        UpdateSettingData(newSet.SettingKey, newSet.SettingValue);

        ExecuteSetting(newSet.SettingKey, newSet.SettingValue);
    }

	private void UpdateSettingData(string settingKey, string value)
    {
        prevSettingDic[settingKey] = value;
    }


    private void ExecuteSetting(string settingKey, string settingValue)
	{
        bool isValidSetting = SettingMethodDic.ContainsKey(settingKey);

        if (isValidSetting)
        {
            SettingMethodDic[settingKey](settingValue);
        }
        else
        {
            //print("NO METHOD TO ADJUST: " + settingKey);
        }
    }


	#endregion


	#region Setting Methods

	private readonly Dictionary<string, Action<string>> SettingMethodDic = new Dictionary<string, Action<string>>()
    {
        // Display
        { "Resolution", SetResolution }, { "Full Screen", SetFullScreen },
        { "Brightness", SetBrightness}, { "Anti-Aliasing", SetAntiAliasing },
        { "Monitor Refresh Rate", SetRefreshRate },
        
        // Game Key
        //

        // Audio & Languages
        { "Master Volume", SetMasterVolume }, { "Music Volume", SetMusicVolume },
        { "Effect Volume", SetEffectVolume }, { "Dialogue Volume", SetDialogueVolume },
        { "Language", SetLangauge }

        // GamePad
        //
    };


	    #region Display
	    private static bool isFullScreen = true;
        private static void SetResolution(string value)
	    {
            string[] splited = value.Split('x');

            if(int.TryParse(splited[0], out int w) && int.TryParse(splited[1], out int h))
		    {
                Screen.SetResolution(w, h, isFullScreen);
                // TODO: ResolutionInfo.Canvas의 Scale Factor 업데이트
                // TODO: ResolutionInfo Scriptable Object 만들기
            } else
		    {
                print("[실패][해상도 설정]: 해상도 숫자 입력 오류");
		    }
        }
        private static void SetFullScreen(string value)
        {
            isFullScreen = bool.Parse(value);

            if (value == true.ToString())
                Screen.SetResolution(Screen.width, Screen.height, true);
            else
                Screen.SetResolution(Screen.width, Screen.height, false);
        }

	    private static void SetBrightness(string value)
	    {
            // only supported in ios
            //Screen.brightness = (int.Parse(value) / 10f);

            // print("Brightness: " + Screen.brightness);
	    }

        private static void SetAntiAliasing(string value)
        {
            if (!Camera.main) return;

            if(Camera.main.gameObject.TryGetComponent<HDAdditionalCameraData>(out var HDCData))
		    {
                switch (value)
                {
                    case "TAA":
                        HDCData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                        break;
                    case "FXAA":
                        HDCData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                        break;
                    case "OFF":
                        HDCData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                        break;
                    default:
                        print("[실패][Anti-Aliasing 설정]: Anti-Aliasing 입력값 오류");
                        break;
                }
            }
        }

	    private static void SetRefreshRate(string value)
        {
            value = value.Substring(0, value.IndexOf("Hz"));

            if (int.TryParse(value, out var rate))
            {
                if (QualitySettings.vSyncCount != 0) QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = rate;
            } else
		    {
                print("[실패][주사율 설정]: 주사율 입력값 오류");
            }
        }

        #endregion

        #region Audio & Languages
        [HideInInspector, SerializeField] private AudioMixerManager audioMixerObj;
	    private static AudioMixerManager statAudioMixerObj;

        private void UpdateAllVolumes()
	    {
            UpdateVolume(AudioGroup.Master);
            UpdateVolume(AudioGroup.Music);
            UpdateVolume(AudioGroup.Effect);
            UpdateVolume(AudioGroup.Dialogue);

        }

        private void UpdateVolume(AudioGroup audioFloatName)
	    {
            var floatName = statAudioMixerObj.GetAudioFloatName(audioFloatName);

            statAudioMixerObj.SetVolume(audioFloatName, prevSettingDic[floatName]);
        }

        private static void SetMasterVolume(string value)
	    {
            statAudioMixerObj.SetVolume(AudioGroup.Master, value);
        }
        private static void SetMusicVolume(string value)
        {
            statAudioMixerObj.SetVolume(AudioGroup.Music, value);
        }
	    private static void SetEffectVolume(string value)
        {
            statAudioMixerObj.SetVolume(AudioGroup.Effect, value);
        }
        private static void SetDialogueVolume(string value)
        {
            statAudioMixerObj.SetVolume(AudioGroup.Dialogue, value);
        }


        private static DialogueInfo statDialogueInfo;
        private static void SetLangauge(string value)
        {
            switch(value)
		    {
                case "KOREAN":
                    statDialogueInfo.language = Language.Korean;

                    break;
                case "ENGLISH":
                    statDialogueInfo.language = Language.English;

                    break;
                case "JAPANESE":
                    statDialogueInfo.language = Language.Japanese;

                    break;
                default:
                    statDialogueInfo.language = Language.English;

                    break;
            }

            if(OnLanguageChanged != null) OnLanguageChanged();
        }

	#endregion


	#endregion


	#region In GameScene

    private GameObject playerCanvas;
    private List<GameObject> childs = new();


	/// <summary> 
    /// Game Scene 상태에서만 작동, 세팅 패널 ON/OFF 
    /// </summary>
	private void ManageInGameSettingPanel()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameModeManager.CurrentGameMode == GameMode.InGame
          && PlayerStatusManager.CurrentInterStatus == InteractionStatus.None)
            {
                GameModeManager.SetGameMode(GameMode.OutGame);

                EnableSetting();
            }
            else
            if (GameModeManager.CurrentGameMode == GameMode.OutGame)
            {
                // if(PreviousPanel == Setting)
                DisableSetting();
                SaveSettingData();

                // else if(PreviousPanel == Reward)
                //
                // ...

                GameModeManager.SetGameMode(GameMode.InGame);
            }
        }
    }


	private void EnableSetting()
	{
		foreach (var child in childs)
			child.SetActive(true);

		playerCanvas.SetActive(false);
	}
	private void DisableSetting()
	{
		foreach (var child in childs)
			child.SetActive(false);

		playerCanvas.SetActive(true);
	}


	#endregion



}
