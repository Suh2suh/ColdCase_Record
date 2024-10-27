using IE.RichFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraChange : MonoBehaviour
{
    [SerializeField] private GameObject vcamera_2;
    [SerializeField] private BoxCollider vc_colliader;
    [SerializeField] private Volume posteffect;
    [SerializeField] private AudioSource elec_sound;
    private Glitch glitch;
    private Vignette vignette;
    private void Start()
    {
        vcamera_2.SetActive(false);
        posteffect.profile.TryGet<Glitch> (out glitch);
        posteffect.profile.TryGet<Vignette> (out vignette);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other == vc_colliader)
        {
            vcamera_2.gameObject.SetActive(true);
            StartCoroutine(glith_effect());
        }

    }

    IEnumerator glith_effect()
    {
        elec_sound.Play();
        for (float i = 0; i < 0.5; i+=0.01f)
        {
            glitch.drift.value = i;
            glitch.jitter.value = i;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.15f);

        for (float i = 0.5f; i >= 0; i -= 0.01f)
        {
            glitch.drift.value = i;
            glitch.jitter.value = i;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.03f);

        for (float i = 0; i < 0.5; i += 0.01f)
        {
            vignette.intensity.value = i;
            
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(0.03f);

        yield return null;
    }

}
