using UnityEngine;
using UnityEngine.UI;

public class ItemDeleteHandler : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject deleteButton;
    [SerializeField] private GameObject confirmOverlay;
    [SerializeField] private Button trashIconBtn;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;

    void Start()
    {
        trashIconBtn.onClick.AddListener(ShowConfirmation);
        yesBtn.onClick.AddListener(ConfirmDelete);
        noBtn.onClick.AddListener(CancelDelete);

        confirmOverlay.SetActive(false);
        deleteButton.SetActive(true);
    }

    void ShowConfirmation()
    {
        confirmOverlay.SetActive(true);
        deleteButton.SetActive(false);
        
        // Disable dragging while deciding
        var dragScript = GetComponent<DraggableItem>();
        if (dragScript != null) dragScript.enabled = false;
    }

    void CancelDelete()
    {
        confirmOverlay.SetActive(false);
        deleteButton.SetActive(true);

        // Re-enable dragging
        var dragScript = GetComponent<DraggableItem>();
        if (dragScript != null) dragScript.enabled = true;
    }

    void ConfirmDelete()
    {
        Destroy(gameObject);
    }
}