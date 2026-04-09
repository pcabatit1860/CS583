using UnityEngine;

/// <summary>
/// Creates an infinitely scrolling background by moving two background images left
/// and resetting them when they go off-screen.
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [Header("Background Settings")]
    public Transform background1;   // First background image
    public Transform background2;   // Second background image (placed to the right of first)
    public float scrollSpeed = 2f;  // How fast the background scrolls left
    public float backgroundWidth = 20f;  // Width of each background piece
    
    private float startX;  // Starting X position of the first background
    
    void Start()
    {
        // Remember where the first background started
        startX = background1.position.x;
    }
    
    void Update()
    {
        // Move both backgrounds to the left
        background1.Translate(Vector2.left * scrollSpeed * Time.deltaTime);
        background2.Translate(Vector2.left * scrollSpeed * Time.deltaTime);
        
        // If background 1 has moved too far left, move it to the right of background 2
        if (background1.position.x < startX - backgroundWidth)
        {
            background1.position = new Vector3(background2.position.x + backgroundWidth, background1.position.y, background1.position.z);
        }
        
        // If background 2 has moved too far left, move it to the right of background 1
        if (background2.position.x < startX - backgroundWidth)
        {
            background2.position = new Vector3(background1.position.x + backgroundWidth, background2.position.y, background2.position.z);
        }
    }
}
