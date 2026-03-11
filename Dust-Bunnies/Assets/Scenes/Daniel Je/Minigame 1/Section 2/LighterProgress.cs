using UnityEngine;

public class LighterProgress : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private LighterProgressBar progressBar;
    [SerializeField]
    private float _threshold = 30f;
    [SerializeField]
    private float increaseRate = 0.2f;
    [SerializeField]
    private float decreaseRate = 0.15f;
    private float _progress = 0;
    private Vector3 pos;
    private bool isSparklerLit = false;
    // public AK.Wwise.Event sparklerLoop;

    void Update()
    {
        //Tip of candle logic
        pos = target.position - transform.position;
        pos.Normalize();
        pos *= 6;
        pos -= new Vector3(0, 5, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        rectTransform.position = screenPos;

        //mouse detection logic
        Vector3 mousePos = Input.mousePosition;
        Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);
        float distance = Vector2.Distance(mousePos, targetScreenPos);
        if (_progress == 1)
        {
            _progress = 1;
            //add win logic here
        }
        else
        {
            if (distance <= _threshold)
            {
                _progress += increaseRate * Time.deltaTime;
            }
            else
            {
                _progress -= decreaseRate * Time.deltaTime;
            }
        }

        if (_progress >= 1f && !isSparklerLit)
        {
            isSparklerLit = true;
            SetSparklerState(true);
        }

        _progress = Mathf.Clamp01(_progress);
        progressBar.SetProgress(_progress);
    }
    public void SetSparklerState(bool isLit)
    {
        if (isLit)
        {
            SFXManager.PlaySFX(SFXManager.Events.SparklerLoop, gameObject);
            // AkUnitySoundEngine.PostEvent("Play_Sparkler_Loop", gameObject);
            // sparklerLoop.Post(gameObject);
        }
    }
}
