using UnityEngine;

public class DeskOverlookZone : MonoBehaviour
{
    [SerializeField] private Collider col;
    [SerializeField] public float triggerDirectionMinAngle = 70f;
    [SerializeField] public float triggerDirectionMaxAngle = 110f;
}
