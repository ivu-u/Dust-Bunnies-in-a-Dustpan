using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Transform playerCam;

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //if (Physics.Raycast(playerCam.position, playerCam.forward, ))
        }
    }
}
