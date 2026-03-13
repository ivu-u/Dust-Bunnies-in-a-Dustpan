using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class JournalItemFocus : MonoBehaviour, IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private float focusScale = 2.0f;
    [SerializeField] private float animationSpeed = 10f;

    // State Tracking
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale;
    private bool isFocused = false;

    // Static Reference (So items know about the Focus Layer)
    public static Transform FocusContainer;
    public static GameObject FocusDimmer;
    public static JournalItemFocus CurrentFocusedItem;

    void Start()
    {
        if (FocusContainer == null)
        {
            GameObject containerObj = GameObject.Find("FocusContainer");
            if (containerObj != null) FocusContainer = containerObj.transform;
        }
        
        if (FocusDimmer == null)
        {
            GameObject dimmerObj = GameObject.Find("FocusDimmer");
            if (dimmerObj != null) FocusDimmer = dimmerObj;
        }
        
        if (FocusDimmer != null)
        {
            FocusDimmer.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (isFocused)
        {
            // Clean up focus state without animation
            if (FocusDimmer != null) FocusDimmer.SetActive(false);
            if (CurrentFocusedItem == this) CurrentFocusedItem = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;

        if (!isFocused)
            Focus();
        else
            Unfocus();
    }

    public void Focus()
    {
        // Save Original State
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
        originalScale = transform.localScale;

        // Move to Focus Layer
        transform.SetParent(FocusContainer);
        
        // Disable Dragging
        var dragScript = GetComponent<DraggableItem>();
        if (dragScript != null) dragScript.enabled = false;

        // Show Dimmer
        if (FocusDimmer != null) 
        {
            FocusDimmer.SetActive(true);
            // Ensure FocusContainer is rendered above the dimmer
            FocusDimmer.transform.SetAsLastSibling();
            FocusContainer.SetAsLastSibling();
        }

        // Animate
        CurrentFocusedItem = this;
        isFocused = true;
        StopAllCoroutines();
        StartCoroutine(AnimateTo(Vector3.zero, Quaternion.identity, Vector3.one * focusScale));
    }

    public void Unfocus()
    {
        // Hide Dimmer
        if (FocusDimmer != null) FocusDimmer.SetActive(false);

        // Restore Parent & Index
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);

        // Re-enable Dragging
        var dragScript = GetComponent<DraggableItem>();
        if (dragScript != null) dragScript.enabled = true;

        // Animate Back
        CurrentFocusedItem = null;
        isFocused = false;
        StopAllCoroutines();
        StartCoroutine(AnimateTo(originalPos, originalRot, originalScale));
    }

    // Static helper for the Dimmer Button to call
    public static void GlobalUnfocus()
    {
        if (CurrentFocusedItem != null) CurrentFocusedItem.Unfocus();
    }

    // Smooth Animation Coroutine
    IEnumerator AnimateTo(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale)
    {
        float t = 0;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        // Snap to finish
        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        transform.localScale = targetScale;
    }
}