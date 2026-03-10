using UnityEngine;

/// <summary>
/// Base Object for everything that can be interacted with.
/// </summary>
public class Interactable : MonoBehaviour
{
    private Vector3 _startPos;  // store initial start pos to return back to
    private Transform _t;

    void Start() {
        _t = transform;
        _startPos = transform.position;
    }

    public void Rotate() {

    }
}
