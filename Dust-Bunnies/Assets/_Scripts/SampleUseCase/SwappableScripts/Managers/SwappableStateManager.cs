using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEditor;

public class SwappableStateManager : MonoBehaviour {

    public event System.Action<SwapState> OnSwapStateSet;

    [SerializeField] private Swappable[] swappableObjects;

    public void PropagateState(SwapState swapState) {
        foreach (Swappable so in swappableObjects) {
            so.SetState(swapState);
        }
        OnSwapStateSet?.Invoke(swapState);
    }

    #if UNITY_EDITOR

    [System.Serializable]
    public class EDITOR_SwappableManagerDebugEntry {
        #if ENABLE_INPUT_SYSTEM
        public Key keyCode;
        #elif ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode keyCode;
        #endif
        public SwapState swapState;
    }

    public EDITOR_SwappableManagerDebugEntry[] stateDebugKeys;

    void Update() {
        foreach (EDITOR_SwappableManagerDebugEntry debugKey in stateDebugKeys) {
            #if ENABLE_INPUT_SYSTEM
            if (Keyboard.current[debugKey.keyCode].wasPressedThisFrame) {
                PropagateState(debugKey.swapState);
            }
            #elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(debugKey.keyCode)) {
                PropagateState(debugKey.swapState);
            }
            #endif
        }
    }

    public void EDITOR_SetSwappableObjects(Swappable[] swappableObjects) {
        Undo.RecordObject(this, "Set Swappable Objects");
        this.swappableObjects = swappableObjects;
        EditorUtility.SetDirty(this);
    }

    #endif
}
