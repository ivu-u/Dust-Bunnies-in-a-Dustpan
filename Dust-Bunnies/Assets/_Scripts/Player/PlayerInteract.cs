using UnityEngine;
using PrimeTween;

/// <summary>
/// Handles player interaction with Interactables (objects)
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform hold_point;
    [SerializeField] private float moveTime = 1f;        // how long it takes to hold an object
    [SerializeField] private float rotationSpeed = 1f;

    private Interactable obj;   // what was picked up
    
    /// <summary>
    /// Called when the player hits the interact button
    /// </summary>
    public Interactable TryPickUp() {
        // shoot ray from the center of camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
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

        // lets do this scuff first
        Transform t = obj.transform;
        Tween.Position(t, hold_point.position, moveTime, Ease.InSine);    // move to player hold point

        // calculate rotation to point towards player cam
        Quaternion rot = Quaternion.LookRotation(playerCam.transform.position - t.position, Vector3.up);
        Tween.Rotation(t, rot, moveTime, Ease.InSine);
        //Tween.EulerAngles(t, t.rotation, t.)
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
        t.Rotate(rot.x * rotationSpeed * playerCam.up, Space.Self);
        t.Rotate(-rot.y * rotationSpeed * Vector3.right, Space.Self);
    }
}
