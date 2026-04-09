using UnityEngine;

/// <summary>
/// This script is attached to animal part collectibles (bat wings, cheetah shoes, turtle shell).
/// It handles the floating animation, collection trigger, and spawning sound effects.
/// </summary>
public class AnimalPart : MonoBehaviour
{
    // These are set in the Inspector when creating the prefab
    public AnimalType animalType;   // Which animal is this? (Bat, Cheetah, or Turtle)
    public string animalName;       // Display name like "Bat Wings"
    public Sprite partSprite;       // The actual image of the animal part
    
    [Header("Floating Animation")]
    public float floatSpeed = 2f;      // How fast the part bobs up and down
    public float floatHeight = 0.2f;   // How high the part bobs
    
    private Vector2 startPos;           // Starting position (where the part spawned)
    private SpriteRenderer spriteRenderer;
    private bool isCollected = false;   // Prevents double collection
    private Collider2D partCollider;    // The trigger collider
    
    void Start()
    {
        // Remember where we started (for floating animation)
        startPos = transform.position;
        
        // Set up the sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        if (partSprite != null)
            spriteRenderer.sprite = partSprite;
        
        // Make sure we have a trigger collider so the player can collect us
        partCollider = GetComponent<Collider2D>();
        if (partCollider == null)
        {
            partCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        partCollider.isTrigger = true;  // Triggers don't block movement, but detect collisions
        
        Debug.Log($"AnimalPart ready: {animalName} (Type: {animalType})");
    }
    
    void Update()
    {
        // Gentle floating/bobbing animation
        // This uses a sine wave to move up and down smoothly
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }
    
    /// <summary>
    /// When the player touches this animal part, they collect it!
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't collect twice
        if (isCollected) return;
        
        // Only the player can collect us
        if (other.CompareTag("Player"))
        {
            isCollected = true;
            
            // Play the collection sound effect
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCollect();
            
            // Tell the AnimalAssembler to equip this part
            AnimalAssembler assembler = other.GetComponent<AnimalAssembler>();
            if (assembler != null)
            {
                Debug.Log($"🎁 Collecting: {animalName} (Type: {animalType})");
                assembler.AddAnimalPart(this);
            }
            
            // Destroy the collectible object (it's been collected!)
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// What type of animal part is this?
/// Used to determine what ability the player gets.
/// </summary>
public enum AnimalType
{
    None,     // No animal part equipped
    Bat,      // Bat wings = double jump
    Cheetah,  // Cheetah shoes = dash ability
    Turtle    // Turtle shell = protection from one hit
}
