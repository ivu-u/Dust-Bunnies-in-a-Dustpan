using UnityEngine;
using PrimeTween;
using System.Collections;

/// <summary>
/// Handles player interaction with Interactables (objects)
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform hold_point;

    [Header("Settings")]
    [SerializeField] private float moveTime = 1f;        // how long it takes to hold an object
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float pickUpDistance = 10f;

    private Interactable obj;   // what was picked up

    private bool _isPickingUp = false;
    public bool IsPickingUp => _isPickingUp;
    
    /// <summary>
    /// Called when the player hits the interact button
    /// </summary>
    public Interactable TryPickUp() {
        // shoot ray from the center of camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpDistance)) {
            if (hit.transform.TryGetComponent(out Interactable interactable)) {
                obj = interactable;
                return interactable;
            }
        }
        return null;
    }

    /// <summary>
    /// First time you interact the player picks up the obj
    /// </summary>
    public void PickUpObj() {
        if (obj == null) { Debug.Log("No object to pick up??"); return; }

        _isPickingUp = true;
        StartCoroutine(PickUp());
    }

    private IEnumerator PickUp() {
        // lets do this scuff first
        Transform t = obj.transform;
        Tween.Position(t, hold_point.position, moveTime, Ease.InSine);    // move to player hold point

        // calculate rotation to point towards player cam
        Quaternion rot = Quaternion.LookRotation(playerCam.transform.position - t.position, Vector3.up);
        Tween.Rotation(t, rot, moveTime, Ease.InSine);
        
        yield return new WaitForSeconds(moveTime);
        _isPickingUp = false;
    }

    public void PutDownObj() {
        if (obj == null) { Debug.Log("Not holding an object???"); return; }

        Transform t = obj.transform;
        Tween.Position(t, obj.StartPos, moveTime, Ease.OutSine);

        //Quaternion rot = Quaternion.LookRotation()
        Tween.Rotation(t, obj.StartRot, moveTime, Ease.OutSine);
    }

    public void Rotate(Vector2 rot) {
        Transform t = obj.transform;
        t.Rotate(playerCam.up ,-rot.x * rotationSpeed, Space.World);
        t.Rotate(playerCam.right, - rot.y * rotationSpeed, Space.World);
    }
}
