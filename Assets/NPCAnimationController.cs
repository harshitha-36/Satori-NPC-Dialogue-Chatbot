using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    private Vector3 targetPosition = new Vector3(13.093f, 0f, 3.883f); // Target position
    private bool isPlayerFollowing = false;

    // Animator Parameters
    private const string IsTypingParam = "IsTyping";
    private const string IsStandingParam = "IsStanding";
    private const string IsTalkingParam = "IsTalking";
    private const string IsWalkingParam = "IsWalking";
    private const string IsTurningLeftParam = "IsTurningLeft";
    private const string IsPointingParam = "IsPointing";

    void Start()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on NPC!");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component not found on NPC!");
        }

        // Find the player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Ensure the player is tagged as 'Player'.");
        }

        // Disable NavMeshAgent initially
        navMeshAgent.enabled = false;

        // Start in typing state
        SetTypingState(true);
    }

    void Update()
    {
        // Check if the player presses 'T' to start the tour
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTour();
        }

        // Check if the player is following
        if (isPlayerFollowing && Vector3.Distance(transform.position, playerTransform.position) > 5f)
        {
            // Player is not following
            StopTour();
        }

        // Check if the NPC has reached the target position
        if (isPlayerFollowing && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // NPC has reached the target position
            StopTour();
            RotateLeftAndPoint();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player entered the trigger zone
            SetTypingState(false); // Stop typing
            SetStandingState(true); // NPC stands up
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player exited the trigger zone
            SetStandingState(false); // NPC sits down (or goes back to idle)
            SetTypingState(true); // Resume typing
        }
    }

    public void SetTalkingState(bool isTalking)
    {
        animator.SetBool(IsTalkingParam, isTalking);
    }

    private void SetTypingState(bool isTyping)
    {
        animator.SetBool(IsTypingParam, isTyping);
    }

    private void SetStandingState(bool isStanding)
    {
        animator.SetBool(IsStandingParam, isStanding);
    }

    private void StartTour()
    {
        // Start the tour
        isPlayerFollowing = true;
        navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(targetPosition); // Walk to the target position
        SetWalkingState(true); // Start walking animation
    }

    private void StopTour()
    {
        // Stop the tour
        isPlayerFollowing = false;
        navMeshAgent.enabled = false;
        SetWalkingState(false); // Stop walking animation
    }

    private void RotateLeftAndPoint()
    {
        // Rotate left
        SetTurningLeftState(true);
        StartCoroutine(PointAfterRotation());
    }

    private IEnumerator PointAfterRotation()
    {
        // Wait for the rotation to complete
        yield return new WaitForSeconds(1f); // Adjust based on rotation animation duration
        SetTurningLeftState(false);

        // Point at the target
        SetPointingState(true);
        yield return new WaitForSeconds(3f); // Hold the pointing animation
        SetPointingState(false);
    }

    private void SetWalkingState(bool isWalking)
    {
        animator.SetBool(IsWalkingParam, isWalking);
    }

    private void SetTurningLeftState(bool isTurningLeft)
    {
        animator.SetBool(IsTurningLeftParam, isTurningLeft);
    }

    private void SetPointingState(bool isPointing)
    {
        animator.SetBool(IsPointingParam, isPointing);
    }
}