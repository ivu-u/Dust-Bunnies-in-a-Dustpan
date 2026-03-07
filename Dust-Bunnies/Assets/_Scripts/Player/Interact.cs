using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private Player player;

    void Start() {
        player.OnInteractPerformed += OnInteract;
    }

    private void OnInteract() {

    }
}
