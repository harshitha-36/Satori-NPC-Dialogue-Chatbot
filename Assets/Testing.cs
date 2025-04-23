using UnityEngine;
using UnityEngine.AI;

public class Testing : MonoBehaviour
{
    // Animator and NavMeshAgent
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    // Player and destination references
    private Transform playerTransform;
    private Vector3 bossRoomPosition;
    private Vector3 npcStartPosition;
    private Quaternion npcStartRotation;

    // Animator Parameters (Bools)
    private const string IsTypingParam = "IsTyping";
    private const string IsStandingParam = "IsStanding";
    private const string IsTalkingParam = "IsTalking";
    private const string IsWalkingParam = "IsWalking";
    private const string IsTurningLeftParam = "IsTurningLeft";
    private const string IsTurningRightParam = "IsTurningRight";
    private const string HasReachedBossRoomParam = "HasReachedBossRoom";
    private const string IsReturningToSeatParam = "IsReturningToSeat";
    private const string HasSatDownParam = "HasSatDown";

    // Animator Triggers
    private const string PlayerEnteredTrigger = "PlayerEnteredTrigger";
    private const string StartConversationTrigger = "StartConversation";
    private const string StartWalkingTrigger = "StartWalking";
    private const string TurnLeftTrigger = "TurnLeftTrigger";
    private const string TurnRightTrigger = "TurnRightTrigger";
    private const string PointAtBossRoomTrigger = "PointAtBossRoom";
    private const string ReturnToSeatTrigger = "ReturnToSeatTrigger";
    private const string SitDownTrigger = "SitDownTrigger";
    private const string FollowMeTrigger = "FollowMeTrigger";

    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Find player and boss room
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        bossRoomPosition = GameObject.FindGameObjectWithTag("BossRoom").transform.position;

        // Save NPC's starting position and rotation
        npcStartPosition = transform.position;
        npcStartRotation = transform.rotation;

        // Initialize NPC to typing state
        SetTypingState(true);
    }

    void Update()
    {
        // Look at player only when talking or guiding
        if (animator.GetBool(IsTalkingParam) || animator.GetBool(IsWalkingParam))
        {
            LookAtPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player entered trigger area
            animator.SetTrigger(PlayerEnteredTrigger);
            SetTypingState(false); // Stop typing and stand up
            StartConversation(); // Start talking to the player
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player left trigger area
            if (animator.GetBool(IsWalkingParam))
            {
                animator.SetTrigger(FollowMeTrigger); // NPC says "Follow me"
            }
        }
    }

    private void StartConversation()
    {
        // Start talking animation
        animator.SetTrigger(StartConversationTrigger);
        SetTalkingState(true);

        // Example: After a short delay, start guiding the player
        Invoke(nameof(StartGuidingPlayer), 2f);
    }

    private void StartGuidingPlayer()
    {
        // Start walking to the boss room
        animator.SetTrigger(StartWalkingTrigger);
        SetWalkingState(true);

        // Set NavMeshAgent destination
        navMeshAgent.SetDestination(bossRoomPosition);

        // Start checking for arrival at boss room
        StartCoroutine(CheckForBossRoomArrival());
    }

    private System.Collections.IEnumerator CheckForBossRoomArrival()
    {
        while (Vector3.Distance(transform.position, bossRoomPosition) > navMeshAgent.stoppingDistance)
        {
            yield return null; // Wait until NPC reaches the boss room
        }

        // NPC has reached the boss room
        animator.SetTrigger(PointAtBossRoomTrigger);
        SetWalkingState(false);

        // Wait for a few seconds, then return to seat
        yield return new WaitForSeconds(3f);
        ReturnToSeat();
    }

    private void ReturnToSeat()
    {
        // Start returning to seat
        animator.SetTrigger(ReturnToSeatTrigger);
        SetReturningToSeatState(true);

        // Set NavMeshAgent destination to starting position
        navMeshAgent.SetDestination(npcStartPosition);

        // Start checking for arrival at seat
        StartCoroutine(CheckForSeatArrival());
    }

    private System.Collections.IEnumerator CheckForSeatArrival()
    {
        while (Vector3.Distance(transform.position, npcStartPosition) > navMeshAgent.stoppingDistance)
        {
            yield return null; // Wait until NPC reaches the seat
        }

        // NPC has reached the seat
        animator.SetTrigger(SitDownTrigger);
        SetReturningToSeatState(false);
        SetTypingState(true); // Resume typing
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    // Helper methods to set Animator parameters
    private void SetTypingState(bool isTyping)
    {
        animator.SetBool(IsTypingParam, isTyping);
    }

    private void SetTalkingState(bool isTalking)
    {
        animator.SetBool(IsTalkingParam, isTalking);
    }

    private void SetWalkingState(bool isWalking)
    {
        animator.SetBool(IsWalkingParam, isWalking);
    }

    private void SetReturningToSeatState(bool isReturning)
    {
        animator.SetBool(IsReturningToSeatParam, isReturning);
    }
}