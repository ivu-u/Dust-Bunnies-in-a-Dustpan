using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;
using TMPro;

public class PlayerJournal : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Settings")]
    private KeyCode journalKey = KeyCode.J;
    
    private KeyCode cameraKey = KeyCode.P;
    private KeyCode escapeKey = KeyCode.Escape;

    [Header("Journal UI")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private GameObject polaroidPrefab;
    [SerializeField] private GameObject stickyNotePrefab;
    [SerializeField] private GameObject photoInboxPanel;
    [SerializeField] private GameObject photosShaderPanel;
    [SerializeField] private Transform inboxContent;
    [SerializeField] private Button togglePhotosButton;
    [SerializeField] private Button newNoteButton;

    [Header("Camera UI")]
    [SerializeField] private GameObject cameraOverlay;
    [SerializeField] private RectTransform captureZone;
    [SerializeField] private Image flashEffect;

    [Header("Input Settings")]
    [SerializeField] private float holdDuration = 0.5f;
    private float holdTimer = 0f;
    private bool hasTriggered = false;

    [Header("UI Feedback")]
    [SerializeField] private Image holdIndicator;

    [Header("Camera Settings")]
    [SerializeField] private float photoCooldown = 1.0f;
    [SerializeField] private float lastPhotoTime = 0f;

    [Header("Camera Lens Settings")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float zoomedFOV = 45f;
    [SerializeField] private float zoomSpeed = 10f;
    
    private Camera mainCam;

    [Header("Flash Settings")]
    [Range(0f, 1f)] [SerializeField] private float maxFlashIntensity = 0.85f;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private Light cameraWorldLight;

    [Header("Book Navigation")]
    [SerializeField] private GameObject[] pageSpreads;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Notification UI")]
    [SerializeField] private GameObject notificationBadge;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("UI Animation")]
    [SerializeField] private RectTransform journalRect;
    [SerializeField] private float centeredX = 0f;
    [SerializeField] private float shiftedX = 125f;
    [SerializeField] private float animationSpeed = 8f;
    private Coroutine slideCoroutine;
    
    // variables to track state
    private int currentPageIndex = 0;
    private float cameraWorldLightIntensity = 10f;
    private bool isJournalOpen = false;
    private bool isCameraMode = false;
    private bool isPhotoInboxOpen = false;
    private bool isPhotosPanelOpen = false;

    void Start()
    {
        journalPanel.SetActive(isJournalOpen);  // journal starts closed
        cameraOverlay.SetActive(isCameraMode);  // camera mode starts off
        photoInboxPanel.SetActive(isPhotoInboxOpen); // inbox starts closed
        if (photosShaderPanel != null) photosShaderPanel.SetActive(isPhotosPanelOpen); // photos panel starts closed
        
        // initialize cursor and player movement for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        
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
        
        // set up main camera reference and default FOV
        mainCam = Camera.main;
        if (mainCam != null) 
            defaultFOV = mainCam.fieldOfView;

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
        
        // initialize hold indicator (starts hidden)
        if (holdIndicator != null)
        {
            holdIndicator.fillAmount = 0f;
            holdIndicator.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PlayerJournal: holdIndicator is not assigned!");
        }
    }

    void Update()
    {
        // Enter Journal Mode if J is pressed, but only if user not already in the journal
        if (Input.GetKeyDown(journalKey) && !isJournalOpen && !isCameraMode) ToggleJournal();

        // Enter Camera Mode if P is pressed, or take photo if already in camera mode
        if (isCameraMode)
        {
            // In camera mode: single press takes a photo
            if (Input.GetKeyDown(cameraKey) && !isJournalOpen)
            {
                if (Time.time - lastPhotoTime >= photoCooldown)
                {
                    StartCoroutine(CapturePhoto());
                    lastPhotoTime = Time.time;
                }
            }
        }
        else
        {
            // Not in camera mode: hold to enter camera mode
            if (Input.GetKey(cameraKey) && !isJournalOpen && !hasTriggered)
            {
                holdTimer += Time.deltaTime;
                
                // Update UI (Ring)
                if (holdIndicator != null)
                {
                    holdIndicator.gameObject.SetActive(true);
                    holdIndicator.fillAmount = holdTimer / holdDuration;
                }

                // Check if held long enough
                if (holdTimer >= holdDuration)
                {
                    ToggleCameraMode();
                    hasTriggered = true;
                    if (holdIndicator != null) holdIndicator.gameObject.SetActive(false);
                }
            }
            
            // Reset when key is released
            if (Input.GetKeyUp(cameraKey))
            {
                holdTimer = 0f;
                hasTriggered = false;
                
                if (holdIndicator != null)
                {
                    holdIndicator.fillAmount = 0f;
                    holdIndicator.gameObject.SetActive(false);
                }
            }
        }

        if (mainCam != null)
        {
            float target = isCameraMode ? zoomedFOV : defaultFOV;
            
            // If we are not at the target, slide towards it
            if (Mathf.Abs(mainCam.fieldOfView - target) > 0.1f)
            {
                mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, target, Time.deltaTime * zoomSpeed);
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
        if (isJournalOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            
            // Ensure FocusDimmer is off when opening journal
            if (JournalItemFocus.FocusDimmer == null)
            {
                GameObject dimmerObj = GameObject.Find("FocusDimmer");
                if (dimmerObj != null) JournalItemFocus.FocusDimmer = dimmerObj;
            }
            if (JournalItemFocus.FocusDimmer != null)
                JournalItemFocus.FocusDimmer.SetActive(false);
        }
        else
        {
            StartCoroutine(LockCursorNextFrame());
        }
        journalPanel.SetActive(isJournalOpen);
        Cursor.visible = isJournalOpen;
        
        // disable player movement when journal is open
        if (playerMovementScript != null) playerMovementScript.enabled = !isJournalOpen;
    }

    public void ToggleCameraMode()
    {
        isCameraMode = !isCameraMode;
        cameraOverlay.SetActive(isCameraMode);

        hasTriggered = false;
        holdTimer = 0f;

        StartCoroutine(LockCursorNextFrame());
        
        // TO DO: disable player movement but allow looking around when in camera mode
        // disabling player movement currently disables the entire character controller, including looking around.
    }

    IEnumerator LockCursorNextFrame()
{
    yield return null;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
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
        }

        // Toggle the photos panel
        if (photosShaderPanel != null)
        {
            isPhotosPanelOpen = isPhotoInboxOpen;
            photosShaderPanel.SetActive(isPhotosPanelOpen);
        }
    }

    // --- Sticky Note ---
    public void CreateStickyNote()
    {
        GameObject newNote = Instantiate(stickyNotePrefab, journalRect.transform);
        RectTransform rect = newNote.GetComponent<RectTransform>();
        
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero; 
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
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