using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Find the root canvas so we can drag freely
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;

        // Visual feedback
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Unparent temporarily so it draws on top of everything
        // Note: In a real app we might use a dedicated "DragLayer"
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Logic: Did we drop it on the "Page"?
        // Raycast to find what we are hovering over
        GameObject dropTarget = eventData.pointerEnter;
        
        if (dropTarget != null && dropTarget.name == "CurrentPage")
        {
            // Success! Stick it to the page
            transform.SetParent(dropTarget.transform);
        }
        else
        {
            // Failed drop (dropped on floor/air)
            // If it was in the inbox, send it back. 
            // If it was already on the page, keep it there.
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero; // Reset position if returned to inbox
        }
    }
}