using UnityEngine;
using System.Collections.Generic;
using PrimeTween;

public class OpenableInteractables: Interactable
{
    public enum InteractableState
    {
        Open,
        Closed
    }
    [SerializeField] List<GameObject> objectsInDrawer;
    bool containsObjects;
    InteractableState state;
    Vector3 originalPosition;
    

    void Start()
    {
        state = InteractableState.Closed;
        originalPosition = transform.position;

        containsObjects = objectsInDrawer.Count > 0;

        // Check if we have a list of objects that will be in drawer
        foreach (GameObject go in objectsInDrawer)
        {
            go.transform.parent = transform;
        }
    }

    InteractableState GetCurrentState()
    {
        return state;
    }

    // Possible improvement: Make the factor public? because everything will probably not use +1/-1 factor
    public void ChangeState()
    {
        float factor = 0f;
        if (state == InteractableState.Open) 
        {
            state = InteractableState.Closed;
            factor = -1f;
        }
        else {
            state = InteractableState.Open;
            factor = 1f;
        }

        // Change the position of drawer depending on if it was opened/closed
        Tween.Position(transform, new Vector3(transform.position.x + factor, transform.position.y, transform.position.z), 1f, Ease.InSine);

    }
}
