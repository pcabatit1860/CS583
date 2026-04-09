using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates an endless, scrolling ground by spawning ground pieces ahead of the player
/// and destroying pieces that are far behind.
/// </summary>
public class GroundSpawner : MonoBehaviour
{
    [Header("Ground Settings")]
    public GameObject groundPrefab;        // The ground piece to spawn
    public float groundWidth = 10f;        // Width of each ground piece
    public float groundYPosition = -2.5f;  // Y position where ground sits
    public int initialGroundPieces = 5;    // How many pieces to spawn at start
    public float spawnAheadDistance = 30f; // How far ahead to spawn new ground
    
    private Transform player;
    private float lastGroundX;             // X position of the last spawned ground
    private List<GameObject> activeGroundPieces = new List<GameObject>();
    
    void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("GroundSpawner: Player not found!");
            return;
        }
        
        // Delete any existing ground in the scene
        GameObject[] existingGround = GameObject.FindGameObjectsWithTag("Ground");
        foreach (GameObject ground in existingGround)
        {
            Destroy(ground);
        }
        
        // Start spawning ground just behind the player
        lastGroundX = player.position.x - groundWidth;
        
        // Spawn the initial ground pieces
        for (int i = 0; i < initialGroundPieces; i++)
        {
            SpawnGroundPiece();
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Spawn more ground if the player is getting close to the end
        if (player.position.x + spawnAheadDistance > lastGroundX)
        {
            SpawnGroundPiece();
        }
        
        // Clean up ground pieces that are far behind the player
        for (int i = activeGroundPieces.Count - 1; i >= 0; i--)
        {
            if (activeGroundPieces[i] == null)
            {
                activeGroundPieces.RemoveAt(i);
                continue;
            }
            
            float groundRightEdge = activeGroundPieces[i].transform.position.x + (groundWidth / 2);
            if (groundRightEdge < player.position.x - 20f)
            {
                Destroy(activeGroundPieces[i]);
                activeGroundPieces.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Spawns a new ground piece at the appropriate position.
    /// </summary>
    void SpawnGroundPiece()
    {
        // Calculate spawn position (center of the ground piece)
        float spawnX = lastGroundX + (groundWidth / 2);
        Vector3 spawnPos = new Vector3(spawnX, groundYPosition, 0);
        
        // Create the new ground piece
        GameObject newGround = Instantiate(groundPrefab, spawnPos, Quaternion.identity);
        newGround.tag = "Ground";  // Important for player ground detection
        activeGroundPieces.Add(newGround);
        
        // Update the last ground X position
        lastGroundX += groundWidth;
    }
}
