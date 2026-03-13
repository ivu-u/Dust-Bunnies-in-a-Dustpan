using UnityEngine;

/// <summary>
/// Base Object for everything that can be interacted with.
/// </summary>
public class Interactable : MonoBehaviour
{
    private Vector3 _startPos;  // store initial start pos to return back to
    private Quaternion _startRot;

    public Vector3 StartPos => _startPos;
    public Quaternion StartRot => _startRot;

    void Start() {
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

}
