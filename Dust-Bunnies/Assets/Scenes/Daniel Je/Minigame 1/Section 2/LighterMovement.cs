using UnityEngine;

public class LighterMovement : MonoBehaviour
{
    private Transform _lighterTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lighterTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseScreen = Input.mousePosition;

        mouseScreen.z = Mathf.Abs(
            Camera.main.transform.position.z - transform.position.z
        );

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreen);
        worldPos.x += 11f;
        worldPos.y += -0.9f;
        transform.position = worldPos;
    }
}
