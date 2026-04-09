using UnityEngine;

/// <summary>
/// Makes the camera follow the player smoothly.
/// The camera stays at a fixed offset relative to the player's position.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;                    // The player to follow
    public Vector3 offset = new Vector3(-3f, -1f, -10f);  // Camera position relative to player
    public float smoothSpeed = 5f;              // How smoothly the camera follows
    
    void LateUpdate()
    {
        // If no target is assigned, try to find the player automatically
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                return;  // No player found yet
        }
        
        // Calculate where the camera wants to be
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move the camera toward that position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Apply the new position
        transform.position = smoothedPosition;
    }
}
