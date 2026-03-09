using UnityEngine;

public partial class BaseObject : MonoBehaviour
{
    public event System.Action OnModuleDetachment;

    [SerializeField] protected Transform objectBody;
    public Transform ObjectBody => objectBody;

    /// <summary>
    /// Disable all object modules attached to this object;
    /// </summary>
    protected void DetachModules() {
        OnModuleDetachment?.Invoke();
    }

    public void UpdateObjectBody(Transform newBody) {
        objectBody = newBody;
    }
}