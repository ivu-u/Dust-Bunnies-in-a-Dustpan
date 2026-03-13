using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player Motor
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed = 10f;

    [Header("Audio")]
    float stepTimer;
    [SerializeField] float stepInterval = 0.1f;



    public void Move(Vector2 inputVector)
    {
        // TODO: update is moving here
        //if (inputVector.magnitude <= 0) return;

        // TODO: movement accel & deccel
        // rn is just simple translate player
        Vector3 move = new(inputVector.x, 0, inputVector.y);
        move = transform.TransformDirection(move);
        move.Normalize();
        characterController.SimpleMove(move * speed);

        // TODO: update this SFX system
        if (inputVector.magnitude > 0 && SceneManager.GetActiveScene().buildIndex != 2)
        {
            SetMovementState(true);
        }
        else
        {
            SetMovementState(false);    // TODO: bad
        }
    }


    public void SetMovementState(bool moving)   // TODO: Implement this into new system
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
