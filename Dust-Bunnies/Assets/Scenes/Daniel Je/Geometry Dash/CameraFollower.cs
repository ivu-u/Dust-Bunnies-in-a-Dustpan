using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.position.x, 1, -10);
    }
}
