using UnityEngine;

public class CandleTarget : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    private float progress;
    [SerializeField]
    private float bigAmplitude = 0.05f;
    [SerializeField]
    private float smallAmplitude = 0.01f;

    [SerializeField]
    private float bigFrequency = 1.5f;
    [SerializeField]
    private float smallFrequency = 15f;
    private float _baseX;
    private float _seed;
    void Update()
    {
        //Hand Movement
        float time = Time.time;

        float bigNoise = Mathf.PerlinNoise(_seed, time * bigFrequency) - 0.5f;
        float smallNoise = Mathf.PerlinNoise(_seed + 100f, time * smallFrequency) - 0.5f;

        float offset = bigNoise * bigAmplitude + smallNoise * smallAmplitude;

        transform.position = new Vector3(
            _baseX + offset,
            transform.position.y,
            transform.position.z
        );
    }
    void Start()
    {
        
    }
}
