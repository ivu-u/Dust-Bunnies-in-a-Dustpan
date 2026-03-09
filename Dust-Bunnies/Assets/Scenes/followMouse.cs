using UnityEngine;

public class followMouse : MonoBehaviour
{
    public float speed = 8.0f;
    public float distanceFromCamera = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 mouseScreenToWorld = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 position = Vector3.Lerp(transform.position, mouseScreenToWorld, 1.0f - Mathf.Exp(-speed * Time.deltaTime));
        transform.position = position;
    }
}