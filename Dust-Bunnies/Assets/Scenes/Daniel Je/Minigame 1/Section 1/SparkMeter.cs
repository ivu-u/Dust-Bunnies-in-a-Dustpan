using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SparkMeter : MonoBehaviour
{
    [SerializeField]
    private float sparkProgress = 0.05f;
    [SerializeField]
    private Slider sparkMeter;
    [SerializeField]
    private float drainRate = 0.1f;
    [SerializeField]
    private Image sliderImage;
    [SerializeField]
    private float threshold = 0.9f;
    [SerializeField]
    private Image backgroundImage;

    private Color _originalColor;
    private float _meterProgress = 0;

    void Start()
    {
        _originalColor = sliderImage.color;
    }
    private void SetProgress(float progressValue)
    {
        sparkMeter.value = Mathf.Clamp(progressValue, sparkMeter.minValue, sparkMeter.maxValue);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if(_meterProgress > threshold)
            {
                Debug.Log("Winner Winner Chicken Dinner");
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
            _meterProgress += sparkProgress;
        _meterProgress -= Time.deltaTime * drainRate;
        _meterProgress = Mathf.Clamp(_meterProgress, 0, 1);

        if (_meterProgress > threshold)
            sliderImage.color = Color.red;
        else
            sliderImage.color = _originalColor;

        backgroundImage.color = Color.Lerp(Color.yellow, Color.red, _meterProgress*0.7f);
        SetProgress(_meterProgress);
    }
}
