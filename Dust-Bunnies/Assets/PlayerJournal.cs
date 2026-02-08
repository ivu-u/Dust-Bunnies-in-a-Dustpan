using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;

public class PlayerJournal : MonoBehaviour
{
    [Header("UI Assignments")]
    public GameObject journalPanel;
    public Transform inboxContent; // The "Content" object inside ScrollView
    public GameObject polaroidPrefab; // The Prefab we made
    public GameObject stickyNotePrefab; // The Prefab we made

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.J;
    public KeyCode photoKey = KeyCode.P;

    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) ToggleJournal();
        if (Input.GetKeyDown(photoKey) && !isOpen) StartCoroutine(CapturePhoto());
    }

    public void ToggleJournal()
    {
        isOpen = !isOpen;
        journalPanel.SetActive(isOpen);
        
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    // --- PHOTO SYSTEM ---

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

        // 3. Add to UI Inbox IMMEDIATELY
        CreatePolaroid(screenImage, path);
    }

    void CreatePolaroid(Texture2D texture, string path)
    {
        // Spawn the prefab inside the Inbox
        GameObject newPhoto = Instantiate(polaroidPrefab, inboxContent);
        
        // Find the RawImage child and assign the texture
        RawImage imgDisplay = newPhoto.GetComponentInChildren<RawImage>();
        imgDisplay.texture = texture;
    }

    // --- STICKY NOTE SYSTEM ---
    
    // Link this function to your "New Note" Button
    public void SpawnStickyNote()
    {
        // Spawn a note directly on the mouse position (or center of screen)
        GameObject newNote = Instantiate(stickyNotePrefab, journalPanel.transform);
        newNote.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }
}