using IE.RichFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class Scenechange : MonoBehaviour
{
    [SerializeField] private BoxCollider vc_colliader;
    [SerializeField] private Volume posteffect;
    private Glitch glitch;
    private void Start()
    {
        posteffect.profile.TryGet<Glitch>(out glitch);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == vc_colliader)
        {
            StartCoroutine(glith_effect());
        }

    }


    IEnumerator glith_effect()
    {
        for (float i = 0; i < 0.5; i += 0.01f)
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

        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Sequence")
        {
            SceneManager.LoadScene("Stage_1");
        }
        yield return null;
    }
}
