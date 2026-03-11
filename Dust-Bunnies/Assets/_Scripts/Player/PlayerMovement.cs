using System;
using PrimeTween;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{


    [SerializeField] private Player player;
    [SerializeField] private CharacterController characterController;
    //[SerializeField] private Transform camHead;
    //[SerializeField] private Camera playerCamera;
    [SerializeField] private float speed = 10f;

    [Header("Audio")]
    float stepTimer;
    [SerializeField] float stepInterval = 0.1f;


    // Update is called once per frame
    void Update() {
        Move(player.MovementVector);
    }

    private void Move(Vector2 inputVector) {
        // TODO: update is moving here
        //if (inputVector.magnitude <= 0) return;

        // TODO: movement accel & deccel
        // rn is just simple translate player
        Vector3 move = new(inputVector.x, 0, inputVector.y);
        move = transform.TransformDirection(move);
        move.Normalize();
        characterController.SimpleMove(move * speed);
    }


    public void SetMovementState(bool moving)
    {
        if (!moving)
        {
            stepTimer = 0;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0;
            SFXManager.PlaySFX(SFXManager.Events.Footstep);
        }
    }
}
