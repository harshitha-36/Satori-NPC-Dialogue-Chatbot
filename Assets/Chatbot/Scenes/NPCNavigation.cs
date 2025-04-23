using UnityEngine;
using UnityEngine.AI;

public class NPCNavigation : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    public Transform destination; // Set this in the Inspector or dynamically

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (destination != null)
        {
            MoveToDestination(destination.position);
        }
    }

    public void MoveToDestination(Vector3 targetPosition)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(targetPosition);
            Debug.Log("NPC is moving to: " + targetPosition);
        }
    }

    void Update()
    {
        // Check if the NPC has reached the destination
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            Debug.Log("NPC has reached the destination.");
        }
    }
}