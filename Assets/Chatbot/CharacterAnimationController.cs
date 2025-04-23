using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>(); // Get Animator component
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Detect player entering trigger area
        {
            animator.SetBool("IsStanding", true);   // Transition from sitting to standing
            Invoke("StartDancing", 1.5f);  // Delay before dancing
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Detect player leaving trigger area
        {
            animator.SetBool("IsDancing", false); // Stop dancing
            animator.SetBool("IsStanding", false); // Return to sitting
        }
    }

    private void StartDancing()
    {
        animator.SetBool("IsDancing", true);  // Start dance after standing up
    }
}
