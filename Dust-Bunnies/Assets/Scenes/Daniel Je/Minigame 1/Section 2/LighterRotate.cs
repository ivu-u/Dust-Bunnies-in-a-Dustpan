using UnityEngine;

public class LighterRotate : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Vector3 axisOffset = new Vector3(90, 0, 0);

    void Update()
    {
        transform.LookAt(target.position);
        transform.Rotate(axisOffset); 
    }
}