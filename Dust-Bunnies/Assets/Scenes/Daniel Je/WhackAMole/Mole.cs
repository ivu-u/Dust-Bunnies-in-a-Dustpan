using UnityEngine;
using System.Collections;

public class Mole : MonoBehaviour
{
    private Vector3 _downPosition;
    private Vector3 _upPosition;

    private bool _isUp;
    private bool _isAnimating;

    private WhackAMoleGameManager _gameManager;

    [SerializeField] private float raiseHeight = 1.5f;
    [SerializeField] private float moveSpeed = 5f;

    public void Initialize(WhackAMoleGameManager manager)
    {
        _gameManager = manager;

        _downPosition = transform.position;
        _upPosition = _downPosition + Vector3.up * raiseHeight;
    }

    public void Raise(float stayDuration)
    {
        if (_isAnimating) return;
        StartCoroutine(RaiseRoutine(stayDuration));
    }

    private IEnumerator RaiseRoutine(float stayDuration)
    {
        _isAnimating = true;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(_downPosition, _upPosition, t);
            yield return null;
        }

        _isUp = true;

        yield return new WaitForSeconds(stayDuration);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(_upPosition, _downPosition, t);
            yield return null;
        }

        _isUp = false;
        _isAnimating = false;
    }

    public void Hit()
    {
        if (!_isUp) return;

        _isUp = false;
        StopAllCoroutines();
        StartCoroutine(ForceDown());

        _gameManager.AddScore(1);
    }

    private IEnumerator ForceDown()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed * 2f;
            transform.position = Vector3.Lerp(transform.position, _downPosition, t);
            yield return null;
        }

        _isAnimating = false;
    }
    
    public bool GetIsUp()
    {
        return _isUp;
    }
}