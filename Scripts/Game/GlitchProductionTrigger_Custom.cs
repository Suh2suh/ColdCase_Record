using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GlitchProductionTrigger_Custom : MonoBehaviour
{
    [SerializeField] private char phase;
    [SerializeField] private GlitchData glitchData;
    [SerializeField] private float time;
    [SerializeField] private GlitchNarrationMessagePlayer_Custom glitchNarrationMessagePlayer;



    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other) && CurrentPhase.Equals(phase))
        {
            if (glitchNarrationMessagePlayer.IsPlaying)
            {
                glitchNarrationMessagePlayer.StopAll();
            }
            glitchNarrationMessagePlayer.PlayGlitchProduction(glitchData, time);
            Destroy(gameObject);
        }
    }



    private bool IsPlayer(Collider other)
    {
        if (other.TryGetComponent<ObjInteractor>(out var objInteractor))
            return true;
        else
            return false;
    }
    private char CurrentPhase
    {
        get
        {
            return PhaseChecker.GetCurrentPhase();
        }
    }



    #region ADDED_TEMPRORY

    public void ForcePlayGlitchProduction()
    {
        if (glitchNarrationMessagePlayer.IsPlaying)
        {
            glitchNarrationMessagePlayer.StopAll();
        }
        glitchNarrationMessagePlayer.PlayGlitchProduction(glitchData, time);
        Destroy(gameObject);
    }

    #endregion


}