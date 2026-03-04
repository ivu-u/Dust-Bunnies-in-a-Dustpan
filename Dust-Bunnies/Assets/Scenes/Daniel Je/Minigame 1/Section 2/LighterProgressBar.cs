using UnityEngine;
using UnityEngine.UI;

public class LighterProgressBar : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] private float progress;
    private Image _fillImage;

    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        _fillImage.fillAmount = progress;
    }

    void Start()
    {
        _fillImage = GetComponent<Image>();
        SetProgress(0);
    }
}