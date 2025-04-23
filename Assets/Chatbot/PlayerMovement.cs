using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public float turnSpeed = 100f; // Speed of rotation

    private void Update()
    {
        // Get input from arrow keys or WASD
        float moveDirection = Input.GetAxis("Vertical"); // Up/Down or W/S
        float turnDirection = Input.GetAxis("Horizontal"); // Left/Right or A/D

        // Move the player forward/backward
        transform.Translate(Vector3.forward * moveSpeed * moveDirection * Time.deltaTime);

        // Rotate the player left/right
        transform.Rotate(Vector3.up * turnSpeed * turnDirection * Time.deltaTime);
    }
}
