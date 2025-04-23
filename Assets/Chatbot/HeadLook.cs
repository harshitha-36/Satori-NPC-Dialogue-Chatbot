using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLook : MonoBehaviour
{
    public Transform HeadObject; // The head bone or object to rotate
    public Transform TargetObject; // The target to look at
    public float LookSpeed; // Speed of head rotation

    public float MaxAngle = 60f; // Maximum angle the head can rotate
    public float MinAngle = -60f; // Minimum angle the head can rotate

    private bool isLooking;
    private Quaternion LastRotation;
    private Quaternion InitialRotation; // Stores the initial forward rotation of the head
    private float HeadResetTimer;

    void Start()
    {
        isLooking = false;
        // Store the initial forward rotation of the head
        InitialRotation = HeadObject.rotation;
    }

    void LateUpdate()
    {
        // Calculate the direction to the target
        Vector3 Direction = (TargetObject.position - HeadObject.position).normalized;

        // Calculate the angle between the head's forward direction and the target direction
        float angle = Vector3.SignedAngle(Direction, HeadObject.forward, HeadObject.up);

        // Check if the target is within the allowed angle range
        if (angle < MaxAngle && angle > MinAngle)
        {
            if (!isLooking)
            {
                isLooking = true;
                LastRotation = HeadObject.rotation;
            }

            // Smoothly rotate the head towards the target
            Quaternion TargetRotation = Quaternion.LookRotation(Direction);
            LastRotation = Quaternion.Slerp(LastRotation, TargetRotation, LookSpeed * Time.deltaTime);
            HeadObject.rotation = LastRotation;

            // Reset the timer
            HeadResetTimer = 0.5f;
        }
        else if (isLooking)
        {
            // Smoothly reset the head to its initial rotation
            LastRotation = Quaternion.Slerp(LastRotation, InitialRotation, LookSpeed * Time.deltaTime);
            HeadObject.rotation = LastRotation;

            // Decrease the timer
            HeadResetTimer -= Time.deltaTime;

            // If the timer runs out, reset the head to its initial rotation
            if (HeadResetTimer <= 0)
            {
                HeadObject.rotation = InitialRotation;
                isLooking = false;
            }
        }
    }
}