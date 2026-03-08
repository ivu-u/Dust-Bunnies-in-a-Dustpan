using System;
using PrimeTween;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform camHead;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float speed = 10f;

    [Header("Free Look Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float lookSmoothing = 10f;
    [SerializeField] private float freeReturnTweenDuration = 1f;

    [Header("Locked Overhead Settings")]
    [SerializeField] private float overheadForwardOffset = 0.3f;
    [SerializeField] private float overheadDownAngle = 60f;
    [SerializeField] private float overheadFOVReduction = 10f;
    [SerializeField] private float overheadEnterAngle = 35f;
    [SerializeField] private float overheadExitMouseYThreshold = 0.1f;
    [SerializeField] private float overheadTweenDuration = 1f;

    [Header("Locked Look Around Settings")]
    [SerializeField] private float lookAroundTiltAngle = 15f;
    [SerializeField] private float lookAroundSideOffset = 0.2f;
    [SerializeField] private float lookAroundFOVReduction = 5f;
    [SerializeField] private float lookAroundTweenDuration = 0.5f;

    enum CamState {
        Free,
        LockedOverhead,
        ReturnFree,
        LockedLookAroundLeft,
        LockedLookAroundRight
    }

    
    private CamState currentCamState = CamState.Free;
    private Vector3 originalCamLocalPos;
    private Vector3 overheadCamLocalPos;
    private Vector3 lookAroundLeftCamLocalPos;
    private Vector3 lookAroundRightCamLocalPos;
    private float originalFOV;
    private float currRotationSpeedZ = 0f;
    private float currRotationSpeedX = 0f;
    private float mouseYIntegrator = 0f;


    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        originalCamLocalPos = camHead.localPosition;
        overheadCamLocalPos = originalCamLocalPos + new Vector3(0, 0, overheadForwardOffset);
        lookAroundLeftCamLocalPos = originalCamLocalPos + new Vector3(-lookAroundSideOffset, 0, 0);
        lookAroundRightCamLocalPos = originalCamLocalPos + new Vector3(lookAroundSideOffset, 0, 0);
        originalFOV = playerCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update() {
        // Translate player
        Vector3 move = new(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        move = transform.TransformDirection(move);
        move.Normalize();
        characterController.SimpleMove(move * speed);

        // Rotate player left/right with mouse X movement, with smoothing
        currRotationSpeedZ = Mathf.Lerp(currRotationSpeedZ, Input.GetAxis("Mouse X") * rotationSpeed, Time.deltaTime * lookSmoothing);
        transform.Rotate(Vector3.up * currRotationSpeedZ);

        // Manual Cam Controls
        if (Input.GetKeyDown(KeyCode.Q) && currentCamState == CamState.Free) {
            SetCamStateLookAroundLeft();
        } else if (Input.GetKeyDown(KeyCode.E) && currentCamState == CamState.Free) {
            SetCamStateLookAroundRight();
        } else if ((Input.GetKeyUp(KeyCode.Q) && currentCamState == CamState.LockedLookAroundLeft) 
                || (Input.GetKeyUp(KeyCode.E) && currentCamState == CamState.LockedLookAroundRight)) {
            SetCamStateReturnFree();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<DeskOverlookZone>(out DeskOverlookZone zone)) {
            CollideWithDeskOverlookZone(zone, false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<DeskOverlookZone>(out DeskOverlookZone zone)) {
            CollideWithDeskOverlookZone(zone, true);
        }
    }

    // LateUpdate is used to ensure camera updates happen after all movement/rotation in Update
    void LateUpdate() {
        if (currentCamState == CamState.Free) {
            CamFree();
        }
    }

    // Called on Trigger while player is in the zone and on exit
    public void CollideWithDeskOverlookZone(DeskOverlookZone zone, bool exiting) {
        if (currentCamState == CamState.LockedOverhead)
        {
            mouseYIntegrator += Time.deltaTime * Input.GetAxis("Mouse Y");
            if (mouseYIntegrator > overheadExitMouseYThreshold
                    || !AngleInRange(transform.localEulerAngles.y, zone.triggerDirectionMinAngle, zone.triggerDirectionMaxAngle)
                    || exiting)
            {
                SetCamStateReturnFree();
            }
        } else {
            float angleToZone = transform.localEulerAngles.y;
            if (AngleInRange(angleToZone, zone.triggerDirectionMinAngle, zone.triggerDirectionMaxAngle)
                && currentCamState == CamState.Free
                && camHead.localEulerAngles.x > overheadEnterAngle && camHead.localEulerAngles.x < 90f) {
                SetCamStateLockedOverhead();
            }
        }
    }
    
    private void CamFree() {
        Vector3 e = camHead.localEulerAngles;
        currRotationSpeedX = Mathf.Lerp(currRotationSpeedX, Input.GetAxis("Mouse Y") * rotationSpeed, Time.deltaTime * lookSmoothing);
        e.x -= currRotationSpeedX;
        e.x = RestrictAngle(e.x, -85f, 85f);
        camHead.localEulerAngles = e;
    }
    
    private void CamLockedOverhead() {
        Sequence.Create()
            .Group(Tween.LocalPosition(camHead, overheadCamLocalPos, duration: overheadTweenDuration, ease: Ease.InOutQuad))
            .Group(TweenCamRotationX(overheadDownAngle, duration: overheadTweenDuration, ease: Ease.InOutQuad))
            .Group(Tween.CameraFieldOfView(playerCamera, originalFOV - overheadFOVReduction, duration: overheadTweenDuration, ease: Ease.Linear));
    }

    private void CamReturnFree() {
        Sequence.Create()
            .Group(Tween.LocalPosition(camHead, originalCamLocalPos, duration: freeReturnTweenDuration, ease: Ease.InOutQuad))
            .Group(TweenCamRotationX(0f, duration: freeReturnTweenDuration, ease: Ease.InOutQuad))
            .Group(TweenCamRotationZ(0f, duration: freeReturnTweenDuration, ease: Ease.InOutQuad))
            .Group(Tween.CameraFieldOfView(playerCamera, originalFOV, duration: freeReturnTweenDuration, ease: Ease.Linear))
            .OnComplete(target: this, target => target.SetCamStateFree());
    }

    private void CamLookAround() {
        Sequence.Create()
            .Group(Tween.LocalPosition(camHead, 
                currentCamState == CamState.LockedLookAroundLeft ? lookAroundLeftCamLocalPos : lookAroundRightCamLocalPos, 
                duration: lookAroundTweenDuration, ease: Ease.InOutQuad))
            .Group(TweenCamRotationX(0, duration: lookAroundTweenDuration, ease: Ease.InOutQuad))
            .Group(TweenCamRotationZ(currentCamState == CamState.LockedLookAroundLeft ? lookAroundTiltAngle : -lookAroundTiltAngle,
                duration: lookAroundTweenDuration, ease: Ease.InOutQuad))
            .Group(Tween.CameraFieldOfView(playerCamera, originalFOV - lookAroundFOVReduction, duration: lookAroundTweenDuration, ease: Ease.Linear));
    }
    

    public void SetCamStateFree() { currentCamState = CamState.Free; currRotationSpeedX = 0f;}
    public void SetCamStateLockedOverhead() { currentCamState = CamState.LockedOverhead; mouseYIntegrator = 0f; CamLockedOverhead();}
    public void SetCamStateReturnFree() { currentCamState = CamState.ReturnFree; CamReturnFree();}
    public void SetCamStateLookAroundLeft() { currentCamState = CamState.LockedLookAroundLeft; CamLookAround();}
    public void SetCamStateLookAroundRight() { currentCamState = CamState.LockedLookAroundRight; CamLookAround();}

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

    private Tween TweenCamRotationX(float endValue, float duration, Easing ease) {
        float startValue = camHead.localEulerAngles.x;
        if (startValue > 180f) startValue -= 360f;
        return Tween.Custom(startValue, endValue, duration: duration, onValueChange: (float val) => {
            Vector3 e = camHead.localEulerAngles;
            e.x = val;
            camHead.localEulerAngles = e;
        }, ease: ease);
    }

    private Tween TweenCamRotationZ(float endValue, float duration, Easing ease) {
        float startValue = camHead.localEulerAngles.z;
        if (startValue > 180f) startValue -= 360f;
        return Tween.Custom(startValue, endValue, duration: duration, onValueChange: (float val) => {
            Vector3 e = camHead.localEulerAngles;
            print(e.z + " to " + val);
            e.z = val;
            camHead.localEulerAngles = e;
        }, ease: ease);
    }
}
