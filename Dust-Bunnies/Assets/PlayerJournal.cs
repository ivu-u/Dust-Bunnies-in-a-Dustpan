using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;

public class PlayerJournal : MonoBehaviour
{
    [Header("Player Settings")]
    public MonoBehaviour playerMovementScript;

    [Header("UI Assignments")]
    public GameObject journalPanel;
    public Transform inboxContent;
    public GameObject polaroidPrefab;
    public GameObject stickyNotePrefab;

    [Header("Settings")]
    public KeyCode journalKey = KeyCode.J;
    
    public KeyCode escapeKey = KeyCode.Escape;
    public KeyCode photoKey = KeyCode.P;

    private bool isOpen = false;

    [Header("Book Navigation")]
    public GameObject[] pageSpreads;
    public Button nextButton;
    public Button prevButton;
    
    private int currentPageIndex = 0;

    [Header("Delete Confirmation")]
    public GameObject deleteConfirmPanel;
    private GameObject itemPendingDeletion;
    private DraggableItem draggableScriptReference;

    void Start()
    {
        journalPanel.SetActive(isOpen);
        UpdatePageVisibility();
    }

    void Update()
    {
        if (Input.GetKeyDown(journalKey) && !isOpen) ToggleJournal();
        if (Input.GetKeyDown(photoKey) && !isOpen) StartCoroutine(CapturePhoto());
        if (Input.GetKeyDown(escapeKey) && isOpen) ToggleJournal();
    }

    public void ToggleJournal()
    {
        isOpen = !isOpen;
        journalPanel.SetActive(isOpen);
        
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !isOpen;
        }
    }

    // --- Photo ---

    IEnumerator CapturePhoto()
    {
        yield return new WaitForEndOfFrame();

        // 1. Capture
        Texture2D screenImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();

        // 2. Save to Disk
        byte[] bytes = screenImage.EncodeToPNG();
        string filename = $"img_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);

        // 3. Add to UI Inbox
        CreatePolaroid(screenImage, path);
    }

    void CreatePolaroid(Texture2D texture, string path)
    {
        GameObject newPhoto = Instantiate(polaroidPrefab, inboxContent);
        
        RawImage imgDisplay = newPhoto.GetComponentInChildren<RawImage>();
        imgDisplay.texture = texture;
    }

    // --- Sticky Note ---
    public void SpawnStickyNote()
    {
        GameObject newNote = Instantiate(stickyNotePrefab, journalPanel.transform);
        newNote.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    public void NextPage()
    {
        if (currentPageIndex < pageSpreads.Length - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
        }
    }

    void UpdatePageVisibility()
    {
        for (int i = 0; i < pageSpreads.Length; i++)
        {
            pageSpreads[i].SetActive(i == currentPageIndex);
        }
        
        if(prevButton) prevButton.interactable = (currentPageIndex > 0);
        if(nextButton) nextButton.interactable = (currentPageIndex < pageSpreads.Length - 1);
    }

    public void RequestDelete(GameObject item, DraggableItem script)
    {
        itemPendingDeletion = item;
        draggableScriptReference = script;
        
        // Show the panel
        deleteConfirmPanel.SetActive(true);
        
        itemPendingDeletion.transform.SetParent(deleteConfirmPanel.transform);
    }

    public void ConfirmDelete()
    {
        if (itemPendingDeletion != null)
        {
            Destroy(itemPendingDeletion);
        }
        CloseConfirmPanel();
    }

    public void CancelDelete()
    {
        if (draggableScriptReference != null)
        {
            draggableScriptReference.RestorePosition();
        }
        CloseConfirmPanel();
    }

    void CloseConfirmPanel()
    {
        itemPendingDeletion = null;
        draggableScriptReference = null;
        deleteConfirmPanel.SetActive(false);
    }
}