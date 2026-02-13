using UnityEngine;

public class DeskOverlookZone : MonoBehaviour
{
    [SerializeField] private Collider col;
    [SerializeField] public float triggerDirectionMinAngle = 70f;
    [SerializeField] public float triggerDirectionMaxAngle = 110f;

    private PlayerMovement player;

    private void OnTriggerEnter(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null) {
            this.player = player;
        }
    }

    private void OnTriggerExit(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null) {
            this.player = null;
            player.CollideWithDeskOverlookZone(this, true);
        }
    }

    private void Update() {
        if (player != null) {
            player.CollideWithDeskOverlookZone(this, false);
        }
    }
}
