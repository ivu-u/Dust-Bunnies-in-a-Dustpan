using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private Player player;

    void Start() {
        player.OnInteractPerformed += OnInteract;
    }

    private void OnInteract() {
        // shoot ray from the center of camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            if (hit.transform.TryGetComponent(out Interactable interactable)) {
                Debug.Log("wow. that's an interactable object.");
            }
        }
    }
}
