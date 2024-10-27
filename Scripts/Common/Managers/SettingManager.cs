using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: ���� �ű��
using Newtonsoft.Json;

using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.SceneManagement;


public class SettingManager : MonoBehaviour
{
    // �ش� ��ũ��Ʈ�� ������ �̸��� ����� ���� ���� -> �ϴ� UI ���� �� ���� ����

    [SerializeField] SceneInfo sceneInfo;
    [SerializeField] DialogueInfo dialogueInfo;

    public static Action OnLanguageChanged;

	#region Unity Methods

	void Awake()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;

        //print("Setting Awake"); // -> DoNotDestroy��, Scene �̵����� Awake()�� ���
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

        if (sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Lobby)
        {
            foreach (GameObject child in childs)
                if (!child.activeSelf)   child.SetActive(true);
        }
        else
		{
            foreach (GameObject child in childs)
                if (child.activeSelf) child.SetActive(false);

            if (sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Game)
            {
                GameObject Player = GameObject.Find("Player");
                playerControlLimiter = Player.GetComponentInChildren<InteractionLimitController>();
                playerCanvas = Player.GetComponentInChildren<Canvas>().gameObject;

                //isGamePaused = false;
            }
        }
		#endregion

		if (!this.gameObject.activeSelf) this.gameObject.SetActive(true);



        #region Setting Methods - Audio & Languages
        // TODO: Awake()���� ���� ������ ����, Start()���� ����Ǵ� ���� �� ����... �ٵ� Start()���� �ϸ� ������ �Ѿ� ����� �Ѥ�
        UpdateAllVolumes(); // Audio Mixer�� Setting Manager���� ���߿� �ʱ�ȭ��. -> �� ���߿� �ٸ� ���� �ű�ų� �ϱ�

        #endregion

    }

 
    void Update()
    {
        if(sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Game)
            ManageInGameSettingPanel();
	}

	private void OnDisable()
	{
        SaveSettingData();
    }

	#endregion



	// Proto, Will be Edited
	#region Setting Data: Initilaization, Load & Save

	Dictionary<string, string> prevSettingDic = new();
    Dictionary<string, ItemController> ItemDic = new();


    // ItemController ������ ȣ��� -> �׳� ItemController�� ���⼭ Set�ϴ� �� ���� �� -> �ߺ��Ǵϱ�
    void InitializeItemList()
	{
        List<ItemController> itemControllers = new();
		GetComponentsInChildren(true, itemControllers);

        foreach (var item in itemControllers)
        {
            item.InitializeItemInfo();

            //ItemDic.Add(item.SettingKey, item);
            if(item.SettingKey != null && item.SettingKey != "")   // settingKey �� ���� �ֵ� ������
                ItemDic[item.SettingKey] = item;
        }

        //Debug.Log("Successfully Initialized Item List");
    }

    void InitializeSettingData()
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


    void LoadSettingData()
	{
        prevSettingDic = JsonConvert.DeserializeObject<Dictionary<string, string>>( DataManager.ReadData(DataManager.settingDataPath) );

        //Debug.Log("Successfully Loaded Setting Data: ");

        foreach(var itemSet in ItemDic)
		{
            var itemKey = itemSet.Key;
            var item = itemSet.Value;

            if (prevSettingDic.ContainsKey(itemKey))
			{
                // ���� �����̶��, record ���� �� setting value ������Ʈ
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

        Debug.Log("Setting Data Exists");
    }


    /// <summary>
    /// Called when the player pressed esc on the setting panel == close the setting panel.
    /// CONSIDER: Save should be when exit game / exit the scene? OR close the panel like now?
    /// </summary>
    void SaveSettingData()
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

    void UpdateSettingData(string settingKey, string value)
    {
        prevSettingDic[settingKey] = value;
    }

    /// <summary>
    /// ���� ���� ���� �Լ�
    /// TODO: Start()�� ����Ǿ���ϴ� ������ �и��� ��. ex) Audio
    /// </summary>
    void ExecuteSetting(string settingKey, string settingValue)
	{
        bool isValidSetting = SettingMethodDic.ContainsKey(settingKey);

        if (isValidSetting)
        {
            SettingMethodDic[settingKey](settingValue);
        }
        else
        {
            print("NO METHOD TO ADJUST: " + settingKey);
        }
    }


    #endregion


    #region Setting Methods

    //public delegate void ParameterizedDelegate(string value);
    
    readonly Dictionary<string, Action<string>> SettingMethodDic = new Dictionary<string, Action<string>>()
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

    /*

    Dictionary<string, Action<string>> testDic = new()
    {
        { "test", test }
    };

    static void test(string value)
	{
        
	}
    */ // �ϴ� ���߿� ���� -> static���� �ٲٱ�

    // TODO: ���� Application�� ���缭 �ٸ��� �����Ű��
    #region Display

    static bool isFullScreen = true;
    static void SetResolution(string value)
	{
        string[] splited = value.Split('x');

        if(int.TryParse(splited[0], out int w) && int.TryParse(splited[1], out int h))
		{
            Screen.SetResolution(w, h, isFullScreen);
            // TODO: ResolutionInfo.Canvas�� Scale Factor ������Ʈ
            // TODO: ResolutionInfo Scriptable Object �����
        } else
		{
            print("[����][�ػ� ����]: �ػ� ���� �Է� ����");
		}
    }
    static void SetFullScreen(string value)
    {
        isFullScreen = bool.Parse(value);

        if (value == true.ToString())
            Screen.SetResolution(Screen.width, Screen.height, true);
        else
            Screen.SetResolution(Screen.width, Screen.height, false);
    }

    static void SetBrightness(string value)
	{
        // only supported in ios
        //Screen.brightness = (int.Parse(value) / 10f);

        // print("Brightness: " + Screen.brightness);
	}

    static void SetAntiAliasing(string value)
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
                    print("[����][Anti-Aliasing ����]: Anti-Aliasing �Է°� ����");
                    break;
            }
        }
    }
    static void SetRefreshRate(string value)
    {
        value = value.Substring(0, value.IndexOf("Hz"));

        if (int.TryParse(value, out var rate))
        {
            if (QualitySettings.vSyncCount != 0) QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = rate;
        } else
		{
            print("[����][�ֻ��� ����]: �ֻ��� �Է°� ����");
        }
    }

    #endregion

    #region Audio & Languages

    [HideInInspector, SerializeField] AudioMixerManager audioMixerObj;
    static AudioMixerManager statAudioMixerObj;

    void UpdateAllVolumes()
	{
        UpdateVolume(AudioMixerManager.audioType.Master);
        UpdateVolume(AudioMixerManager.audioType.Music);
        UpdateVolume(AudioMixerManager.audioType.Effect);
        UpdateVolume(AudioMixerManager.audioType.Dialogue);

    }

    void UpdateVolume(AudioMixerManager.audioType audioFloatName)
	{
        var floatName = statAudioMixerObj.GetAudioFloatName(audioFloatName);

        statAudioMixerObj.SetVolume(audioFloatName, prevSettingDic[floatName]);
    }

    static void SetMasterVolume(string value)
	{
        statAudioMixerObj.SetVolume(AudioMixerManager.audioType.Master, value);
    }
    static void SetMusicVolume(string value)
    {
        statAudioMixerObj.SetVolume(AudioMixerManager.audioType.Music, value);
    }
    static void SetEffectVolume(string value)
    {
        statAudioMixerObj.SetVolume(AudioMixerManager.audioType.Effect, value);
    }
    static void SetDialogueVolume(string value)
    {
        statAudioMixerObj.SetVolume(AudioMixerManager.audioType.Dialogue, value);
    }


    static DialogueInfo statDialogueInfo;
    static void SetLangauge(string value)
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


    #region Manage Setting Panel Activation

    InteractionLimitController playerControlLimiter;
    GameObject playerCanvas;

    List<GameObject> childs = new();


    /// <summary> Game Scene ���¿����� �۵�, ���� �г� ON/OFF </summary>
    void ManageInGameSettingPanel()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameModeManager.GetCurrentGameMode() == GameMode.Game
          && PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
            {
                GameModeManager.SetGameMode(GameMode.Setting);

                EnableSetting();
            }
            else
            if (GameModeManager.GetCurrentGameMode() == GameMode.Setting)
            {
                DisableSetting();

                SaveSettingData();

                GameModeManager.SetGameMode(GameMode.Game);
            }
        }
    }

    void EnableSetting()
	{
        //if (sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Game)
          //  PlayerStatusManager.SetInterStatus(InteractionStatus.Setting);

        foreach (var child in childs) child.SetActive(true);

        playerCanvas.SetActive(false);
    }
    void DisableSetting()
	{
        //if (sceneInfo.GetCurrentSceneInfo() == SceneInfo.Scene.Game)
          //  PlayerStatusManager.SetInterStatus(InteractionStatus.None);

        foreach (var child in childs) child.SetActive(false);

        playerCanvas.SetActive(true);
    }


    

    /// <summary>
    /// If there is some obj/anim that you don't want to stop during setting panel is on,
    /// Make it "Unscaled Time"
    /// </summary>
    /*
    void PauseGame()
	{
        Time.timeScale = 0f;
        playerControlLimiter.UnLockCursor();
    }
    void ResumeGame()
	{
        Time.timeScale = 1f;

        if (PlayerStatusManager.GetCurrentInterStatus() != InteractionStatus.TalkingNpc)
		{
            if (PlayerStatusManager.GetCurrentInterStatus() == InteractionStatus.None)
                playerControlLimiter.LockCursor(isCHShown: true);
            else
                playerControlLimiter.LockCursor(isCHShown: false);
        }
    }
    */

	#endregion

}
