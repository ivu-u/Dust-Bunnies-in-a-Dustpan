using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float fadeTime = 1f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void FadeTo(int index)
    {
        StartCoroutine(FadeOut(index));
    }

    IEnumerator FadeIn()
    {
        float t = fadeTime;

        while (t > 0)
        {
            t -= Time.deltaTime;
            float a = curve.Evaluate(t);
            img.color = new Color(0f, 0f, 0f, t);
            yield return 0;         // wait a frame and then continue
        }
    }

    IEnumerator FadeOut(int index)
    {
        float t = 0f;    // time

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = curve.Evaluate(t);
            img.color = new Color(0f, 0f, 0f, t);
            yield return 0;         // wait a frame and then continue
        }
        switch (index)
        {
            case 0:
                AkUnitySoundEngine.SetState("Scene", "Mirror");
                break;

            case 1:
                AkUnitySoundEngine.SetState("Scene", "Key1");
                break;

            case 2:
                AkUnitySoundEngine.SetState("Scene", "Key2");
                break;

            case 3:
                AkUnitySoundEngine.SetState("Scene", "Key3");
                break;
        }
        SceneManager.LoadScene(index);
    }


}
