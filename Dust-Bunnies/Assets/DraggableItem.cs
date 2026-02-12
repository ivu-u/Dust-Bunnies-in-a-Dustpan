using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    // "Snap Back" Data
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private int originalSiblingIndex;

    public static DraggableItem CurrentlyDragging = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvas = GetComponentInParent<Canvas>();
        
        // Save pre-drag data for potential snap-back
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // move to top of hierarchy for visibility
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        CurrentlyDragging = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        CurrentlyDragging = null;

        GameObject hitObject = eventData.pointerEnter;
        bool dropSuccess = false;

        if (hitObject != null)
        {
            
            if (IsNameInHierarchy(hitObject, "Page"))
            {
                Transform targetTransform = GetTargetInHierarchy(hitObject.transform, "Page");
                RectTransform targetRect = targetTransform.GetComponent<RectTransform>();

                // Strict containment check
                if (IsRectFullyInside(rectTransform, targetRect))
                {
                    transform.SetParent(targetTransform);
                    dropSuccess = true;
                }
                else
                {
                    Debug.Log("Drop Rejected: Item was overlapping the edge!");
                }
            }
        }

        // FAIL: Snap back
        if (!dropSuccess)
        {
            RestorePosition();
        }
    }

    public void RestorePosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalAnchoredPosition;
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    // --- HELPERS ---

    // Checks if item is completely contained within container
    private bool IsRectFullyInside(RectTransform item, RectTransform container)
    {
        Vector3[] itemCorners = new Vector3[4];
        Vector3[] containerCorners = new Vector3[4];

        item.GetWorldCorners(itemCorners);
        container.GetWorldCorners(containerCorners);

        // Calculate Bounds manually to be safe
        float containerMinX = containerCorners[0].x;
        float containerMaxX = containerCorners[2].x;
        float containerMinY = containerCorners[0].y;
        float containerMaxY = containerCorners[2].y;

        // Check all 4 corners of the item
        foreach (Vector3 corner in itemCorners)
        {
            if (corner.x < containerMinX || corner.x > containerMaxX ||
                corner.y < containerMinY || corner.y > containerMaxY)
            {
                return false;
            }
        }
        return true;
    }

    bool IsNameInHierarchy(GameObject obj, string nameToCheck)
    {
        Transform t = obj.transform;
        while (t != null)
        {
            if (t.name.Contains(nameToCheck)) return true;
            t = t.parent;
            if (t != null && t.GetComponent<Canvas>() != null) break;
        }
        return false;
    }

    Transform GetTargetInHierarchy(Transform t, string key1)
    {
        while (t != null)
        {
            if (t.name.Contains(key1)) return t;
            t = t.parent;
            if (t != null && t.GetComponent<Canvas>() != null) break;
        }
        return t;
    }
}