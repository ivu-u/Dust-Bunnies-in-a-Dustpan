using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;
using TMPro;

public class PlayerJournal : MonoBehaviour
{
    [Header("Player Settings")]
    public MonoBehaviour playerMovementScript;

    [Header("Settings")]
    public KeyCode journalKey = KeyCode.J;
    
    public KeyCode cameraKey = KeyCode.P;
    public KeyCode escapeKey = KeyCode.Escape;

    [Header("Journal UI")]
    public GameObject journalPanel;
    public GameObject polaroidPrefab;
    public GameObject stickyNotePrefab;
    public GameObject photoInboxPanel;
    public Transform inboxContent;
    public Button togglePhotosButton;
    public Button newNoteButton;

    [Header("Camera UI")]
    public GameObject cameraOverlay;
    public RectTransform captureZone;
    public Image flashEffect;

    [Header("Camera Settings")]
    public float photoCooldown = 1.0f;
    private float lastPhotoTime = 0f;

    [Header("Flash Settings")]
    [Range(0f, 1f)] public float maxFlashIntensity = 0.85f;
    public float flashDuration = 0.5f;
    public AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public Light cameraWorldLight;

    [Header("Book Navigation")]
    public GameObject[] pageSpreads;
    public Button nextButton;
    public Button prevButton;

    [Header("Notification UI")]
    public GameObject notificationBadge;
    public TextMeshProUGUI notificationText;

    [Header("UI Animation")]
    public RectTransform journalRect;
    public float centeredX = 0f;
    public float shiftedX = 125f;
    public float animationSpeed = 8f;
    private Coroutine slideCoroutine;
    
    // variables to track state
    private int currentPageIndex = 0;
    private float cameraWorldLightIntensity = 10f;
    private bool isJournalOpen = false;
    private bool isCameraMode = false;
    private bool isPhotoInboxOpen = false;

    void Start()
    {
        journalPanel.SetActive(isJournalOpen);  // journal starts closed
        cameraOverlay.SetActive(isCameraMode);  // camera mode starts off
        photoInboxPanel.SetActive(isPhotoInboxOpen); // inbox starts closed
        
        // flash effect should start invisible
        if (flashEffect != null)
        {
            flashEffect.canvasRenderer.SetAlpha(1f);
            Color col = flashEffect.color;
            flashEffect.color = new Color(col.r, col.g, col.b, 0f);
        }

        // camera world light should start off
        if (cameraWorldLight != null) 
        {
            cameraWorldLight.enabled = false;
        }

        // set up toggle inbox button
        if (togglePhotosButton != null)
        {
            togglePhotosButton.onClick.AddListener(ToggleInboxVisibility);
        }

        // set up new note button
        if (newNoteButton != null)
        {
            newNoteButton.onClick.AddListener(CreateStickyNote); 
        }

        // set initial page visibility and button states
        UpdatePageVisibility();
    }

    void Update()
    {
        // Enter Journal Mode if J is pressed, but only if user not already in the journal
        if (Input.GetKeyDown(journalKey) && !isJournalOpen && !isCameraMode) ToggleJournal();

        // Enter Camera Mode if P is pressed, or take photo if already in camera mode
        if (Input.GetKeyDown(cameraKey) && !isJournalOpen)
        {
            if (isCameraMode)
            {
                // take picture if already in camera mode
                if (Time.time - lastPhotoTime >= photoCooldown)
                {
                    StartCoroutine(CapturePhoto());
                    lastPhotoTime = Time.time;
                }
            }
            else
            {
                // enter camera mode
                ToggleCameraMode();
            }
        }

        // Exit journal/camera mode if Escape is pressed
        if (Input.GetKeyDown(escapeKey) && isJournalOpen) ToggleJournal();
        if (Input.GetKeyDown(escapeKey) && isCameraMode) ToggleCameraMode();

        UpdatePhotoCount();
    }

    public void ToggleJournal()
    {
        isJournalOpen = !isJournalOpen;
        journalPanel.SetActive(isJournalOpen);
        
        // handle cursor visibility and lock state
        Cursor.lockState = isJournalOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isJournalOpen;
        
        // disable player movement when journal is open
        if (playerMovementScript != null) playerMovementScript.enabled = !isJournalOpen;
    }

    public void ToggleCameraMode()
    {
        isCameraMode = !isCameraMode;
        cameraOverlay.SetActive(isCameraMode);
        
        // FEATURE: disable player movement but allow looking around when in camera mode
        // disabling player movement currently disables the entire character controller, including looking around.
    }

    // --- Photo ---
    IEnumerator CapturePhoto()
    {
        // 1. enable world light flash effect
        if (cameraWorldLight != null) 
        {
            cameraWorldLight.enabled = true;
            cameraWorldLight.intensity = cameraWorldLightIntensity;
        }

        // 2. chill ;-;
        yield return null;
        yield return new WaitForEndOfFrame();

        // 3. Calculate pixels
        Vector3[] corners = new Vector3[4];
        captureZone.GetWorldCorners(corners);

        // corners[0] is Bottom-Left, corners[2] is Top-Right
        int startX = Mathf.RoundToInt(corners[0].x);
        int startY = Mathf.RoundToInt(corners[0].y);
        
        // Calculate dimensions based on the corners, NOT the rect.width
        int width = Mathf.RoundToInt(corners[2].x - corners[0].x);
        int height = Mathf.RoundToInt(corners[2].y - corners[0].y);

        if (startX < 0) startX = 0;
        if (startY < 0) startY = 0;
        
        if (startX + width > Screen.width) width = Screen.width - startX;
        if (startY + height > Screen.height) height = Screen.height - startY;

        // Verify we have valid dimensions before reading
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Capture Zone is invalid or off-screen!");
            yield break;
        }

        // 4. Capture Img
        Texture2D croppedPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
        croppedPhoto.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        croppedPhoto.Apply();

        // 5. Camera Flash Effect
        StartCoroutine(TriggerFlash());

        // 6. Save to Disk
        byte[] bytes = croppedPhoto.EncodeToPNG();
        string filename = $"img_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);

        // 7. Add to UI Inbox
        CreatePolaroid(croppedPhoto);
    }

    IEnumerator TriggerFlash()
    {
        // flash
        if (flashEffect != null)
        {
            flashEffect.canvasRenderer.SetAlpha(1f);
            flashEffect.color = new Color(1f, 1f, 0.9f, maxFlashIntensity); 
        }

        float elapsed = 0f;
        
        // fade out loop
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / flashDuration;
            
            // subtle color shift 
            if (flashEffect != null)
            {
                float alpha = Mathf.Lerp(maxFlashIntensity, 0f, percent);
                Color c = flashEffect.color;
                flashEffect.color = new Color(c.r, c.g, c.b, alpha);
            }

            // fading
            if (cameraWorldLight != null)
            {
                cameraWorldLight.intensity = Mathf.Lerp(cameraWorldLightIntensity, 0f, percent);
            }

            yield return null;
        }

        // cleanup
        if (cameraWorldLight != null) cameraWorldLight.enabled = false;
        if (flashEffect != null) 
        {
            Color c = flashEffect.color;
            flashEffect.color = new Color(c.r, c.g, c.b, 0f);
        }
    }
    
    void CreatePolaroid(Texture2D texture)
    {
        GameObject newPhoto = Instantiate(polaroidPrefab, inboxContent);
        RawImage imgDisplay = newPhoto.GetComponentInChildren<RawImage>();
        imgDisplay.texture = texture;
    }

    void UpdatePhotoCount()
    {
        if (inboxContent == null || notificationBadge == null) return;

        // Count the photos
        int currentCount = inboxContent.childCount;

        if (currentCount > 0)
        {
            // Show Badge
            notificationBadge.SetActive(true);
            notificationText.text = currentCount.ToString();
        }
        else
        {
            // Hide Badge if empty
            notificationBadge.SetActive(false);
        }
    }

    void ToggleInboxVisibility()
    {
        if (photoInboxPanel != null)
        {
            isPhotoInboxOpen = !isPhotoInboxOpen;
            photoInboxPanel.SetActive(isPhotoInboxOpen);
            
            // stop other animations to prevent conflicts
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            if (isPhotoInboxOpen)
            {
                // Inbox is opening -> Slide Journal RIGHT
                slideCoroutine = StartCoroutine(SlideJournalTo(shiftedX));
            }
            else
            {
                // Inbox is closing -> Slide Journal CENTER
                slideCoroutine = StartCoroutine(SlideJournalTo(centeredX));
            }

            // FEATURE: Add Visual Feedback for the button color depending on state
        }
    }

    // --- Sticky Note ---
    public void CreateStickyNote()
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
        
        // hide buttons if edge of book reached
        if(prevButton) prevButton.gameObject.SetActive(currentPageIndex > 0);
        if(nextButton) nextButton.gameObject.SetActive(currentPageIndex < pageSpreads.Length - 1);
    }

    IEnumerator SlideJournalTo(float targetX)
    {
        Vector2 currentPos = journalRect.anchoredPosition;
        
        while (Mathf.Abs(currentPos.x - targetX) > 1f)
        {
            float newX = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * animationSpeed);
            currentPos.x = newX;
            journalRect.anchoredPosition = currentPos;
            yield return null;
        }
        currentPos.x = targetX;
        journalRect.anchoredPosition = currentPos;        
        slideCoroutine = null;
    }
}