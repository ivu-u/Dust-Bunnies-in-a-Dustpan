using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform camHead;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float lookSmoothing = 5f;
    [SerializeField] private float camTransitionSpeed = 3f;
    
    [Header("Locked Overhead Settings")]
    [SerializeField] private float overheadForwardOffset = 0.3f;
    [SerializeField] private float overheadDownAngle = 60f;
        [Header("Locked Look Around Settings")]
    [SerializeField] private float lookAroundTiltAngle = 15f;
    [SerializeField] private float lookAroundSideOffset = 0.2f;

    enum CamState {
        Free,
        LockedOverhead,
        ReturnFree,
        LockedLookAroundLeft,
        LockedLookAroundRight
    }

    
    private CamState currentCamState = CamState.Free;
    private Vector3 originalCamLocalPos;
    private float originalFOV;
    private float currRotationSpeedZ = 0f;
    private float currRotationSpeedX = 0f;
    private float mouseYIntegrator = 0f;


    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        originalCamLocalPos = camHead.localPosition;
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update() {
        Vector3 move = new(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        move = transform.TransformDirection(move);
        move.Normalize();
        characterController.SimpleMove(move * speed);

        currRotationSpeedZ = Mathf.Lerp(currRotationSpeedZ, Input.GetAxis("Mouse X") * rotationSpeed, Time.deltaTime * lookSmoothing);
        transform.Rotate(Vector3.up * currRotationSpeedZ);

        // Handles returning from locked cam states
        if (currentCamState == CamState.LockedOverhead)
        {
            mouseYIntegrator += Time.deltaTime * Input.GetAxis("Mouse Y");
            if (mouseYIntegrator > 0.1f || Math.Abs(Mathf.DeltaAngle(this.transform.localEulerAngles.y, 90f)) > 20f)
            {
                SetCamStateReturnFree();
            }
        }
    }

    // LateUpdate is used to ensure camera updates happen after all movement/rotation in Update
    void LateUpdate() {
        switch (currentCamState) {
            case CamState.Free:
                CamFree();
                break;
            case CamState.LockedOverhead:
                CamLockedOverhead();
                break;
            case CamState.ReturnFree:
                CamReturnFree();
                break;
            case CamState.LockedLookAroundLeft:
            case CamState.LockedLookAroundRight:
                CamLookAround();
                break;
        }
    }

    // Called by DeskOverlookZone while player is in the zone and on exit
    public void CollideWithDeskOverlookZone(DeskOverlookZone zone, bool exiting) {
        if (exiting) {
            if (currentCamState == CamState.LockedOverhead) {
                SetCamStateReturnFree();
            }
            return;
        }
        float angleToZone = transform.localEulerAngles.y;
        if (AngleInRange(angleToZone, zone.triggerDirectionMinAngle, zone.triggerDirectionMaxAngle)
            && currentCamState == CamState.Free
            && camHead.localEulerAngles.x > 35f && camHead.localEulerAngles.x < 90f) {
            SetCamStateLockedOverhead();
        }
    }
    
    private void CamFree() {
        Vector3 e = camHead.localEulerAngles;
        currRotationSpeedX = Mathf.Lerp(currRotationSpeedX, Input.GetAxis("Mouse Y") * rotationSpeed, Time.deltaTime * lookSmoothing);
        e.x -= currRotationSpeedX;
        e.x = RestrictAngle(e.x, -85f, 85f);
        camHead.localEulerAngles = e;
        
        // Smoothly return to original position
        if (camHead.localPosition != originalCamLocalPos) {
            camHead.localPosition = Vector3.Lerp(camHead.localPosition, originalCamLocalPos, Time.deltaTime * camTransitionSpeed);
        }
        
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, originalFOV, Time.deltaTime * camTransitionSpeed);
    }
    
    private void CamLockedOverhead() {
        // Move camera slightly forward to lean over desk
        Vector3 targetPos = originalCamLocalPos + new Vector3(0, 0, overheadForwardOffset);
        if (camHead.localPosition != targetPos) {
            camHead.localPosition = Vector3.Lerp(camHead.localPosition, targetPos, Time.deltaTime * camTransitionSpeed);
        }
        
        // Pitch down to look at desk
        Vector3 e = camHead.localEulerAngles;
        e.x = Mathf.LerpAngle(e.x, overheadDownAngle, Time.deltaTime * camTransitionSpeed);
        e.z = Mathf.LerpAngle(e.z, 0f, Time.deltaTime * camTransitionSpeed);
        camHead.localEulerAngles = e;
        
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, originalFOV, Time.deltaTime * camTransitionSpeed);
    }

    private void CamReturnFree() {
        // Return to original position
        camHead.localPosition = Vector3.Lerp(camHead.localPosition, originalCamLocalPos, Time.deltaTime * camTransitionSpeed);
        
        // Return to looking straight ahead
        Vector3 e = camHead.localEulerAngles;
        e.x = Mathf.LerpAngle(e.x, 0f, Time.deltaTime * camTransitionSpeed);
        e.z = Mathf.LerpAngle(e.z, 0f, Time.deltaTime * camTransitionSpeed);
        camHead.localEulerAngles = e;
        
        // Return to original FOV
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, originalFOV, Time.deltaTime * camTransitionSpeed);
        
        // Check if close enough to original position and angle to switch to Free state
        print(Vector3.Distance(camHead.localPosition, originalCamLocalPos));
        print(Mathf.Abs(Mathf.DeltaAngle(camHead.localEulerAngles.x, 0f)));
        if (Vector3.Distance(camHead.localPosition, originalCamLocalPos) < 0.1f
            && Mathf.Abs(Mathf.DeltaAngle(camHead.localEulerAngles.x, 0f)) < 5f
            && Mathf.Abs(Mathf.DeltaAngle(camHead.localEulerAngles.z, 0f)) < 0.1f) {
            currentCamState = CamState.Free;
        }
    }

    private void CamLookAround() {
        Vector3 targetPos = originalCamLocalPos + new Vector3(
            currentCamState == CamState.LockedLookAroundLeft ? -lookAroundSideOffset : lookAroundSideOffset,
            0,
            0);
        if (camHead.localPosition != targetPos) {
            camHead.localPosition = Vector3.Lerp(camHead.localPosition, targetPos, Time.deltaTime * camTransitionSpeed);
        }

        Vector3 e = camHead.localEulerAngles;
        float targetTilt = currentCamState == CamState.LockedLookAroundLeft ? lookAroundTiltAngle : -lookAroundTiltAngle;
        e.z = Mathf.LerpAngle(e.z, targetTilt, Time.deltaTime * camTransitionSpeed);
        e.x = Mathf.LerpAngle(e.x, 0f, Time.deltaTime * camTransitionSpeed);
        camHead.localEulerAngles = e;
    }
    

    public void SetCamStateFree() => currentCamState = CamState.Free;
    public void SetCamStateLockedOverhead() { currentCamState = CamState.LockedOverhead; mouseYIntegrator = 0f; }
    public void SetCamStateReturnFree() => currentCamState = CamState.ReturnFree;
    public void SetCamStateLookAroundLeft() => currentCamState = CamState.LockedLookAroundLeft;
    public void SetCamStateLookAroundRight() => currentCamState = CamState.LockedLookAroundRight;

    /// <summary>
    /// Helper method for head rotation, to prevent bending backwards
    /// </summary>
    private float RestrictAngle(float angle, float min, float max) {
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;

        if (angle > max)
            angle = max;
        if (angle < min)
            angle = min;

        return angle;
    }

    private bool AngleInRange(float angle, float min, float max)
    {
        float normalizedAngle = (angle + 360) % 360;
        float normalizedMin = (min + 360) % 360;
        float normalizedMax = (max + 360) % 360;

        if (normalizedMin < normalizedMax)
            return normalizedAngle >= normalizedMin && normalizedAngle <= normalizedMax;
        else
            return normalizedAngle >= normalizedMin || normalizedAngle <= normalizedMax;
    }
}
