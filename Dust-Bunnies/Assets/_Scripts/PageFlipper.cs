using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PageFlipper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private bool isNextButton = true;
    [SerializeField] private float flipInterval = 0.75f;

    private PlayerJournal journal;
    private Coroutine flipCoroutine;
    private bool isHovering = false;

    void Start()
    {
        journal = FindFirstObjectByType<PlayerJournal>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        // Only start if dragging something
        if (DraggableItem.CurrentlyDragging != null)
        {
            if (flipCoroutine == null)
                flipCoroutine = StartCoroutine(FlipPageLoop());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        // Stop immediately when mouse leaves
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }
    }

    IEnumerator FlipPageLoop()
    {
        // Initial delay so it doesn't flip instantly upon entry
        yield return new WaitForSeconds(0.2f);

        // looping for continuous flipping
        while (isHovering && DraggableItem.CurrentlyDragging != null)
        {
            // flip time interval
            yield return new WaitForSeconds(flipInterval);

            if (journal != null)
            {
                if (isNextButton)
                    journal.NextPage();
                else
                    journal.PrevPage();
            }
        }
        flipCoroutine = null;
    }
}