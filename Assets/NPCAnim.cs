using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCAnim : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Vector3 bossRoomPosition;
    private Vector3 npcStartPosition;
    private Quaternion npcStartRotation;

    // Animator Parameters (Bools)
    private const string IsTypingParam = "IsTyping";
    private const string IsWalkingParam = "IsWalking";
    private const string IsTalkingParam = "IsTalking";

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

        // Find the Boss Room position
        GameObject bossRoom = GameObject.FindGameObjectWithTag("BossRoom");
        if (bossRoom != null)
        {
            bossRoomPosition = bossRoom.transform.position;
        }
        else
        {
            Debug.LogError("Boss Room not found! Ensure the boss room is tagged as 'BossRoom'.");
        }

        // Save the NPC's starting position and rotation
        npcStartPosition = transform.position;
        npcStartRotation = transform.rotation;

        // Start in typing state
        SetTypingState(true);
        navMeshAgent.enabled = false; // Disable NavMeshAgent initially

        // Start the sequence: typing -> standing -> walking to Boss Room
        StartCoroutine(StartNPCSequence());
    }

    private IEnumerator StartNPCSequence()
    {
        // NPC is typing for a few seconds
        yield return new WaitForSeconds(5f);

        // NPC stops typing and stands up
        SetTypingState(false);
        yield return new WaitForSeconds(2f); // Wait for the stand-up animation

        // NPC starts walking to the Boss Room
        StartGuidingToBossRoom();
    }

    public void StartGuidingToBossRoom()
    {
        // Enable NavMeshAgent and set destination to Boss Room
        navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(bossRoomPosition);

        // Set walking animation
        SetWalkingState(true);

        // Start checking for arrival at the Boss Room
        StartCoroutine(CheckForBossRoomArrival());
    }

    private IEnumerator CheckForBossRoomArrival()
    {
        // Wait until the NPC reaches the Boss Room
        while (Vector3.Distance(transform.position, bossRoomPosition) > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        // NPC has reached the Boss Room
        SetWalkingState(false);
        Debug.Log("NPC has reached the Boss Room.");

        // Optionally, trigger an animation or action here (e.g., pointing at the Boss Room)
    }

    private void SetTypingState(bool isTyping)
    {
        animator.SetBool(IsTypingParam, isTyping);
    }

    private void SetWalkingState(bool isWalking)
    {
        animator.SetBool(IsWalkingParam, isWalking);
    }

    private void SetTalkingState(bool isTalking)
    {
        animator.SetBool(IsTalkingParam, isTalking);
    }
}