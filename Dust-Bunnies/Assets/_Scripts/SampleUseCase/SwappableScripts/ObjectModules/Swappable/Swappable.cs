using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseObject))]
public class Swappable : ObjectModule {

    public System.Action<SwapState> OnStateSwap;

    [System.Serializable]
    public class SwapStateConfiguration {
        public SwapState swapState;
        public GameObject swapBody;
    }

    [Tooltip("Instantiated swap bodies will be children to this transform if assigned;\n" +
             "Else, the object this component is attached to will serve as the parent for all instantiated bodies;")]
    [SerializeField] private Transform bodyAnchor;
    [Tooltip("Configuration mapping each swap state to all items required for this module to function;\n" +
             "The first state assigned within the list is the default state of this object should no pre-emptive " +
             "state-setting calls be made;")]
    [SerializeField] private SwapStateConfiguration[] stateConfigurations;
    [Tooltip("Whether asset-bound swap bodies should be instantiated on 'Awake' if not already present in the scene;\n" +
             "Otherwise, swap bodies will be instantiated on demand;")]
    [SerializeField] private bool prewarmStates = true;

    private readonly Dictionary<SwapState, SwapStateConfiguration> configMap = new();
    private SwapState currentState;

    protected override void Awake() {
        base.Awake();
        BaseObject.OnTrySwapState += BaseObject_OnTrySwapState;

        if (stateConfigurations.Length > 0) {
            currentState = stateConfigurations[0].swapState;

            foreach (SwapStateConfiguration stateConfig in stateConfigurations) {
                configMap[stateConfig.swapState] = stateConfig;

                if (stateConfig.swapBody) {
                    /// If the assigned swap body is asset-bound, Instantiate the swap body and reference that instead;
                    if (prewarmStates
                            && stateConfig.swapBody.scene.name == null) {
                        InstantiateStateBody(stateConfig);
                    }
                    /// Toggle swap bodies based on the current swap state;
                    stateConfig.swapBody.SetActive(stateConfig.swapState == currentState);
                }
            }
        }

        if (currentState != null) {
            SetState(currentState);
        }
    }

    private bool BaseObject_OnTrySwapState(SwapState targetState) {
        return SetState(targetState);
    }

    /// <summary>
    /// Attempt a state swap;
    /// </summary>
    /// <returns> True if the swap was successful, false otherwise; </returns>
    public bool SetState(SwapState targetState) {
        bool isStateConfigured = configMap.TryGetValue(targetState, out SwapStateConfiguration targetConfig);

        /// No state swap attempt will be performed if this object does not have a valid configuration for said state;
        if (isStateConfigured) {
            /// If states were not prewarmed on 'Awake' and the target swap body is asset bound, instantiate the body;
            if (!prewarmStates && targetConfig.swapBody.scene == null) {
                InstantiateStateBody(targetConfig);
            }

            foreach (KeyValuePair<SwapState, SwapStateConfiguration> kvp in configMap) {
                GameObject swapBody = kvp.Value.swapBody;

                /// Toggle body if the state has a body assigned and said body is not asset-bound;
                if (swapBody && swapBody.scene != null) {
                    swapBody.SetActive(kvp.Key == targetState);
                    BaseObject.UpdateObjectBody(swapBody.transform);
                }
            }

            OnStateSwap?.Invoke(targetState);
            BaseObject.PropagateSwapState(targetState);
        }
        return isStateConfigured;
    }

    private void InstantiateStateBody(SwapStateConfiguration stateConfig) {
        GameObject swapBodyInstance = Instantiate(stateConfig.swapBody, transform.position,
                                                  transform.rotation, bodyAnchor ? bodyAnchor : transform);
        stateConfig.swapBody = swapBodyInstance;
    }

    #if UNITY_EDITOR

    public void EDITOR_PrewarmStates() {
        foreach (SwapStateConfiguration stateConfig in stateConfigurations) {
            if (stateConfig.swapBody) {
                /// If the assigned swap body is asset-bound, Instantiate the swap body and reference that instead;
                if (stateConfig.swapBody.scene.name == null) {
                    GameObject swapBodyInstance = UnityEditor.PrefabUtility.InstantiatePrefab(stateConfig.swapBody) as GameObject;
                    swapBodyInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    swapBodyInstance.transform.SetParent(bodyAnchor ? bodyAnchor : transform);

                    UnityEditor.Undo.RegisterCreatedObjectUndo(swapBodyInstance, "Undo swap body instantiation;");

                    UnityEditor.Undo.RecordObject(this, "Update Swap Bodies");
                    stateConfig.swapBody = swapBodyInstance;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
    }

    #endif
}
