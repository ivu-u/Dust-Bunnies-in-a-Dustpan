using UnityEngine;

public class randomizeEyes : MonoBehaviour
{
    Animator animator;
    float randomIdleStart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = Random.Range(.75f, 1.25f);
        randomIdleStart = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length);
        animator.Play("eye_animation", 0, randomIdleStart);
    }

}
