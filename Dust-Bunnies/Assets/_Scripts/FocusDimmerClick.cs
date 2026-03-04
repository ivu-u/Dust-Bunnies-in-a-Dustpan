using UnityEngine;
using UnityEngine.EventSystems;

public class FocusDimmerClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        JournalItemFocus.GlobalUnfocus();
    }
}
