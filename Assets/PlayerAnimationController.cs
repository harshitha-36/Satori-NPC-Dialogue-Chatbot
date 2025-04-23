using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private float turnSpeed = 120f; // Degrees per second
    private float moveSpeed = 3f; // Units per second

    // Animator Parameters
    private const string IsWalkingParam = "IsWalking";
    private const string IsTurningLeftParam = "IsTurningLeft";
    private const string IsTurningRightParam = "IsTurningRight";

    void Start()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on Player!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleTurning();
    }

    private void HandleMovement()
    {
        // Check for forward/backward movement (Up Arrow / Down Arrow)
        float moveInput = Input.GetAxis("Vertical"); // W/S or Up Arrow/Down Arrow
        if (moveInput != 0)
        {
            // Move the player
            transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

            // Set walking animation
            animator.SetBool(IsWalkingParam, true);
        }
        else
        {
            // Stop walking animation
            animator.SetBool(IsWalkingParam, false);
        }
    }

    private void HandleTurning()
    {
        // Check for left/right turning (Left Arrow / Right Arrow)
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left Arrow/Right Arrow
        if (turnInput != 0)
        {
            // Rotate the player
            transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

            // Set turning animations
            if (turnInput > 0)
            {
                // Turning right
                animator.SetBool(IsTurningRightParam, true);
                animator.SetBool(IsTurningLeftParam, false);
            }
            else if (turnInput < 0)
            {
                // Turning left
                animator.SetBool(IsTurningLeftParam, true);
                animator.SetBool(IsTurningRightParam, false);
            }
        }
        else
        {
            // Stop turning animations
            animator.SetBool(IsTurningLeftParam, false);
            animator.SetBool(IsTurningRightParam, false);
        }
    }
}