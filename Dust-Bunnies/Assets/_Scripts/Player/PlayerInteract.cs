using UnityEngine;
using PrimeTween;

/// <summary>
/// Handles player interaction with Interactables (objects)
/// </summary>
public class Interact : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform hold_point;

    void Start() {
        player.OnInteractPerformed += OnInteract;
        player.OnRotatePerformed += OnRotate;
    }

    /// <summary>
    /// Called when the player hits the interact button
    /// </summary>
    private void OnInteract() {
        // shoot ray from the center of camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {

            // TODO: Create Interact() base func for Interactables that will be overriden for openable interactables
            if (hit.transform.TryGetComponent(out OpenableInteractables oi)) {
                Debug.Log("oh this can be opened.");

                // Change state of openable object
                // if player successfully interacts with openable object --> do NOT deactiveate input actions
                Transform t = oi.transform;
                oi.ChangeState();
            }

            else if (hit.transform.TryGetComponent(out Interactable interactable)) {
                Debug.Log("wow. that's an interactable object.");

                // lets do this scuff first
                Transform t = interactable.transform;
                Tween.Position(t, hold_point.position, 1f, Ease.InSine);

                // if player successfully interacts with something --> switch control action maps
                player.ActivateInteractInputs();    // TODO: change
            }
        }
    }

    private void OnRotate(float val) {
        
    }
}
