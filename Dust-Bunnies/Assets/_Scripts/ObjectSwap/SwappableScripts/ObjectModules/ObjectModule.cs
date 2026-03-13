using UnityEngine;

[RequireComponent(typeof(BaseObject))]
public abstract class ObjectModule : MonoBehaviour
{
    [HideInInspector][SerializeField] private BaseObject baseObject;
    protected BaseObject BaseObject {
        get {
            if (!baseObject) TryGetComponent(out baseObject);
            return baseObject;
        }
    }

    protected virtual void Awake() {
        BaseObject.OnModuleDetachment += Detach;
    }

    /// <summary>
    /// Disable module if requested from the base object;
    /// </summary>
    protected virtual void Detach() {
        enabled = false;
    }

    #if UNITY_EDITOR
    /// Attempt to cache a reference to this module's base object, if none exists;
    protected virtual void OnValidate() {
        if (!baseObject) TryGetComponent(out baseObject);
    }
    #endif
}
