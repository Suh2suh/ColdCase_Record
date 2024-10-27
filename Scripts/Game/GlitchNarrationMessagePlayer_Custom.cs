using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityUtility;

public class GlitchNarrationMessagePlayer_Custom : MonoBehaviour
{
    [SerializeField] private GlitchNarrationMessageData glitchNarrationMessageData;
    private GameObject currentCustomPassVolume;
    private CoroutineQueue glitchNarrationMessageQueue;
    private const string glitchMaterialEmissionValueName = "_Emission_Value";
    private const float maxGlitchMaterialEmissionValue = 1f;
    private const string glitchMaterialOffsetName = "_Offset";
    private const float maxGlitchMaterialOffsetValue = 100f;

    [SerializeField] NarrationSubtitlePlayer narrationSubtitlePlayer;



    private void OnEnable()
    {
        SetGlitchMaterialValue(0f);
    }
    private void Awake()
    {
        glitchNarrationMessageQueue = new CoroutineQueue(this);
    }
    private void Start()
    {
        glitchNarrationMessageQueue.OnCoroutineStart += InstantiateCustomPassVolume;
        glitchNarrationMessageQueue.OnCoroutineEnd += () => Destroy(currentCustomPassVolume);
    }
    private void OnDisable()
    {
        SetGlitchMaterialValue(0f);
    }



    private void InstantiateCustomPassVolume()
    {
        currentCustomPassVolume = Instantiate(glitchNarrationMessageData.CustomPassVolume);
    }
    public void PlayGlitchProduction(GlitchData glitchData, float time)
    {
        glitchNarrationMessageQueue.Enqueue(new GlitchNarrationMessage(glitchData.GlitchObject, GlitchNarrationMessagePlayingCoroutine(glitchData.GlitchObject, Wait())));

        narrationSubtitlePlayer.ShowSubtitleForMoment("Narration", glitchData.GlitchNarrationKey, time);


        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time);
        }
    }
    public void PlayGlitchProduction(GlitchData glitchData, float time, params GameObject[] gameObjectsToDestroyOnEnd)
    {
        PlayGlitchProduction(glitchData, time);
        glitchNarrationMessageQueue.Enqueue(new GameObjectsDestroying(gameObjectsToDestroyOnEnd));
    }

    //public void PlayGlitchNarrationMessage(GameObject glitchObject, narration data)
    private IEnumerator GlitchNarrationMessagePlayingCoroutine(GameObject glitchObject, IEnumerator coroutineBeforeDisappearing)
    {
        if (currentCustomPassVolume == false)
        {
            InstantiateCustomPassVolume();
        }

        glitchObject.SetActive(true);
        AudioPlayer.InstantiateAndPlayAndDestroy(glitchNarrationMessageData.GlitchSound);
        SetGlitchMaterialValue(0);

        yield return coroutineBeforeDisappearing;
        // TODO : yield return play narration


        float lerpValue = 0;
        bool isPlayDisappearingSound = false;
        while (lerpValue < 1f)
        {
            if (lerpValue >= glitchNarrationMessageData.GlitchSoundPlayingValue && isPlayDisappearingSound == false)
            {
                isPlayDisappearingSound = true;
                AudioPlayer.InstantiateAndPlayAndDestroy(glitchNarrationMessageData.GlitchSound);
            }
            lerpValue += Time.deltaTime * glitchNarrationMessageData.GlitchDisappearingSpeed;
            SetGlitchMaterialValue(lerpValue);
            yield return null;
        }

        Destroy(glitchObject);
        SetGlitchMaterialValue(0);
        narrationSubtitlePlayer.ActivateNarrationPanel(false);
    }
    private void SetGlitchMaterialValue(float value)
    {
        glitchNarrationMessageData.GlitchMaterial.SetFloat(glitchMaterialOffsetName, Mathf.Lerp(maxGlitchMaterialOffsetValue, 0, value));
        glitchNarrationMessageData.GlitchMaterial.SetFloat(glitchMaterialEmissionValueName, Mathf.Lerp(0, maxGlitchMaterialEmissionValue, value));
    }
    public void StopAll()
    {
        Destroy(currentCustomPassVolume);
        glitchNarrationMessageQueue.StopAll();
    }
    public void Stop()
    {
        Destroy(currentCustomPassVolume);
        glitchNarrationMessageQueue.StopCurrentCoroutine();
    }
    public bool IsPlaying { get => glitchNarrationMessageQueue.IsPlaying; }
}