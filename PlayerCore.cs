using UnityEngine;
using System.Collections;

/// <summary>
/// This script controls the player's movement, abilities, and collision handling.
/// It's the heart of the player character - handles running, jumping, dashing, and animal power-ups.
/// </summary>
public class PlayerCore : MonoBehaviour
{
    // ========== INSPECTOR SETTINGS ==========
    // These values can be tweaked in the Unity Inspector while the game is running
    
    [Header("Movement Settings")]
    public float autoRunSpeed = 5f;      // How fast the player runs to the right automatically
    public float jumpForce = 14f;         // How high the player jumps when pressing space
    
    [Header("Animal Ability Settings")]
    public float batJumpMultiplier = 1.8f;    // Bat wings make you jump higher (multiplier)
    public float cheetahDashSpeed = 14f;      // How fast the cheetah dash moves
    public float cheetahDashDuration = 0.25f; // How long the dash lasts
    public float cheetahDashCooldown = 1.2f;  // Time before you can dash again
    
    [Header("Ground Check")]
    public Transform groundCheckPoint;    // An empty object at the player's feet to detect ground
    public float groundCheckRadius = 0.25f;   // Size of the ground detection circle
    public LayerMask groundLayer;         // Which layers count as "ground"
    
    [Header("Speed Increase Settings")]
    public float speedIncreaseRate = 0.05f;   // How much speed increases each time (5%)
    public float speedIncreaseInterval = 10f; // How often speed increases (every 10 seconds)
    
    // ========== PRIVATE VARIABLES ==========
    // These track the player's current state during gameplay
    
    private Rigidbody2D rb;               // Reference to the physics component
    private Animator animator;            // Reference to the animation controller
    
    public bool isGrounded { get; private set; }  // Is the player standing on something?
    private float currentJumpForce;       // Current jump power (affected by bat wings)
    
    // Animal ability flags - which powers does the player currently have?
    private bool hasBatWings = false;
    private bool hasCheetahSpeed = false;
    private bool hasTurtleShell = false;
    
    // Double jump mechanics
    private bool canDoubleJump = false;
    private bool hasJumpedThisFrame = false;  // Prevents multiple jumps in one frame
    
    // Cheetah dash mechanics
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float originalRunSpeed;
    private float lastTapTime = 0f;       // For detecting double-tap
    
    // Turtle shell mechanics
    private int turtleShells = 0;          // How many shells the player has
    private bool isInvincible = false;     // Temporary invincibility after getting hit
    
    // Speed increase tracking
    private float speedTimer = 0f;
    private float currentSpeedMultiplier = 1f;
    
    private bool isGameOver = false;       // Has the player died?
    
    // ========== INITIALIZATION ==========
    
    void Start()
    {
        // Get references to required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentJumpForce = jumpForce;
        originalRunSpeed = autoRunSpeed;
        
        // Make sure the player starts on the ground (not falling through)
        StartCoroutine(ForceGroundPosition());
        
        Debug.Log("PlayerCore initialized - ready to run!");
    }
    
    /// <summary>
    /// Waits a moment for ground to spawn, then positions the player on top of it.
    /// This prevents the player from falling through at the start.
    /// </summary>
    IEnumerator ForceGroundPosition()
    {
        // Wait a moment for ground to spawn (0.2 seconds)
        yield return new WaitForSeconds(0.2f);
        
        // Find the ground and place the player on top of it
        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null)
        {
            Collider2D groundCollider = ground.GetComponent<Collider2D>();
            if (groundCollider != null)
            {
                // Calculate where the top of the ground is
                float groundTop = groundCollider.bounds.max.y;
                Collider2D playerCollider = GetComponent<Collider2D>();
                float playerHeight = playerCollider != null ? playerCollider.bounds.size.y : 1f;
                float newY = groundTop + (playerHeight / 2);  // Place player so feet touch ground
                
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                Debug.Log($"Player repositioned to Y: {newY}");
            }
        }
        
        // Reset any falling momentum
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    // ========== GAME LOOP (UPDATE) ==========
    
    void Update()
    {
        // Don't do anything if game is over or paused
        if (isGameOver) return;
        if (Time.timeScale == 0) return;
        
        // Handle dash timer (how long the dash lasts)
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                StopDash();
            }
        }
        
        // Handle dash cooldown (time before next dash)
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        
        // Increase speed over time (game gets harder the longer you survive)
        speedTimer += Time.deltaTime;
        if (speedTimer >= speedIncreaseInterval)
        {
            speedTimer = 0f;
            currentSpeedMultiplier += speedIncreaseRate;
            autoRunSpeed = originalRunSpeed * currentSpeedMultiplier;
            Debug.Log($"⚡ Speed increased! Current speed: {autoRunSpeed:F2}");
        }
        
        // Update jump force based on active abilities
        UpdateAbilityBonuses();
        
        // Auto-run movement (player runs right automatically)
        if (!isDashing)
        {
            transform.Translate(Vector2.right * autoRunSpeed * Time.deltaTime);
        }
        
        // Check if the player is on the ground (using the groundCheckPoint)
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        // Reset double jump when landing
        if (isGrounded && !wasGrounded)
        {
            canDoubleJump = true;
        }
        
        // Handle jump input (Space bar)
        if (Input.GetButtonDown("Jump") && !isDashing && !hasJumpedThisFrame)
        {
            hasJumpedThisFrame = true;
            
            if (isGrounded)
            {
                Jump();  // Normal jump
            }
            else if (hasBatWings && canDoubleJump)
            {
                DoubleJump();  // Double jump (only if you have bat wings)
            }
        }
        else if (Input.GetButtonUp("Jump"))
        {
            hasJumpedThisFrame = false;
        }
        
        // Handle cheetah dash input (double-tap right arrow)
        if (hasCheetahSpeed && !isDashing && dashCooldownTimer <= 0)
        {
            DetectDashInput();
        }
        
        // Update animation parameters (so animations match what's happening)
        if (animator != null && rb != null)
        {
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("HasBatWings", hasBatWings);
            animator.SetBool("IsDashing", isDashing);
        }
    }
    
    /// <summary>
    /// Updates jump force based on whether the player has bat wings.
    /// Bat wings = higher jumps!
    /// </summary>
    void UpdateAbilityBonuses()
    {
        if (hasBatWings)
        {
            currentJumpForce = jumpForce * batJumpMultiplier;
        }
        else
        {
            currentJumpForce = jumpForce;
        }
    }
    
    /// <summary>
    /// Makes the player jump!
    /// </summary>
    void Jump()
    {
        if (rb == null) return;
        
        rb.velocity = new Vector2(rb.velocity.x, currentJumpForce);
        canDoubleJump = true;
        
        // Play jump sound effect
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayJump();
        
        // Trigger jump animation
        if (animator != null)
            animator.SetTrigger("Jump");
        
        Debug.Log($"Jump! Force: {currentJumpForce}");
    }
    
    /// <summary>
    /// Performs a double jump (only possible with bat wings)
    /// </summary>
    void DoubleJump()
    {
        if (rb == null) return;
        
        float doubleJumpForce = currentJumpForce * 0.85f;  // Slightly weaker than normal jump
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        canDoubleJump = false;  // Can only double jump once per air-time
        
        if (animator != null)
            animator.SetTrigger("DoubleJump");
        
        Debug.Log($"Double Jump! Force: {doubleJumpForce}");
    }
    
    /// <summary>
    /// Detects double-tap on the right arrow key for cheetah dash.
    /// </summary>
    void DetectDashInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - lastTapTime < 0.3f)  // Two taps within 0.3 seconds = double tap
            {
                StartDash();
            }
            else
            {
                lastTapTime = Time.time;  // First tap - wait for second tap
            }
        }
    }
    
    /// <summary>
    /// Starts the cheetah dash - a burst of speed!
    /// </summary>
    void StartDash()
    {
        isDashing = true;
        dashTimer = cheetahDashDuration;
        dashCooldownTimer = cheetahDashCooldown;
        rb.velocity = new Vector2(cheetahDashSpeed, rb.velocity.y);
        
        Debug.Log("🐆 Cheetah dash activated!");
        
        if (animator != null)
            animator.SetTrigger("Dash");
    }
    
    /// <summary>
    /// Ends the cheetah dash and returns to normal speed.
    /// </summary>
    void StopDash()
    {
        isDashing = false;
        Debug.Log("Cheetah dash ended");
    }
    
    /// <summary>
    /// Called when the player collides with something.
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGameOver) return;
        
        // If we hit an obstacle, handle it
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            HandleObstacleHit(collision.gameObject);
        }
    }
    
    /// <summary>
    /// Handles what happens when the player hits an obstacle.
    /// Turtle shell protects you, otherwise GAME OVER!
    /// </summary>
    void HandleObstacleHit(GameObject obstacle)
    {
        // Try to use a turtle shell for protection
        if (TryUseTurtleShell())
        {
            Destroy(obstacle);  // Shell destroys the obstacle
            Debug.Log("🐢 Turtle shell saved you!");
            return;
        }
        
        // No shell? GAME OVER!
        Debug.Log("💀 GAME OVER!");
        isGameOver = true;
        this.enabled = false;  // Disable player movement
        
        // Tell the ScoreManager to show the game over screen
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.GameOver();
        }
        else
        {
            Time.timeScale = 0f;  // Pause the game
        }
    }
    
    /// <summary>
    /// Attempts to use a turtle shell for protection.
    /// Returns true if a shell was used, false if no shells left.
    /// </summary>
    public bool TryUseTurtleShell()
    {
        if (hasTurtleShell && turtleShells > 0 && !isInvincible)
        {
            turtleShells--;  // One shell is consumed
            StartCoroutine(InvincibilityFrames());  // Brief invincibility
            
            // Play shell break sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayTurtleShellHit();
            
            Debug.Log($"🐢 Turtle shell used! {turtleShells} left");
            
            // Remove the visual shell from the player
            AnimalAssembler assembler = GetComponent<AnimalAssembler>();
            if (assembler != null)
            {
                assembler.RemoveAnimal();
            }
            
            if (turtleShells <= 0)
            {
                hasTurtleShell = false;
                Debug.Log("🐢 Turtle shell broken!");
            }
            
            return true;  // Hit was absorbed
        }
        return false;  // No protection available
    }
    
    /// <summary>
    /// Makes the player invincible for a short time and flashes their sprite yellow.
    /// </summary>
    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Get all sprite renderers on the player and children
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                originalColors[i] = sprites[i].color;
        }
        
        float duration = 1.5f;  // How long invincibility lasts
        float elapsed = 0f;
        float storedSpeed = autoRunSpeed;
        
        // Flash yellow and white repeatedly
        while (elapsed < duration)
        {
            // Check if sprites still exist (player might have died)
            bool validSprites = true;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                {
                    validSprites = false;
                    break;
                }
            }
            
            if (!validSprites) break;
            
            // Turn yellow
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                    sprites[i].color = Color.yellow;
            }
            yield return new WaitForSeconds(0.1f);
            
            // Check again
            validSprites = true;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                {
                    validSprites = false;
                    break;
                }
            }
            
            if (!validSprites) break;
            
            // Turn back to white
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                    sprites[i].color = Color.white;
            }
            yield return new WaitForSeconds(0.1f);
            
            elapsed += 0.2f;
        }
        
        // Restore original colors
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null && i < originalColors.Length)
                sprites[i].color = originalColors[i];
        }
        
        isInvincible = false;
        autoRunSpeed = storedSpeed;
    }
    
    // ========== PUBLIC METHODS (called by AnimalAssembler) ==========
    
    /// <summary>
    /// Gives the player bat wings ability (double jump + higher jump)
    /// </summary>
    public void EnableBatWings()
    {
        hasBatWings = true;
        Debug.Log("🦇 Bat wings equipped! Double jump unlocked.");
    }
    
    /// <summary>
    /// Gives the player cheetah speed ability (dash on double-tap)
    /// </summary>
    public void EnableCheetahSpeed()
    {
        hasCheetahSpeed = true;
        Debug.Log("🐆 Cheetah speed equipped! Double-tap → to dash!");
    }
    
    /// <summary>
    /// Gives the player turtle shell protection (survive one hit)
    /// </summary>
    public void EnableTurtleShell(int shells = 1)
    {
        hasTurtleShell = true;
        turtleShells += shells;
        Debug.Log($"🐢 Turtle shell equipped! {turtleShells} shell(s)");
    }
    
    /// <summary>
    /// Removes all animal abilities from the player.
    /// Called when an animal part expires or is removed.
    /// </summary>
    public void DisableAllAbilities()
    {
        hasBatWings = false;
        hasCheetahSpeed = false;
        hasTurtleShell = false;
        turtleShells = 0;
        isDashing = false;
        
        Debug.Log("All abilities disabled - speed progression continues!");
    }
    
    // Quick property getters for other scripts to check player status
    public bool HasBatWings() => hasBatWings;
    public bool HasCheetahSpeed() => hasCheetahSpeed;
    public bool HasTurtleShell() => hasTurtleShell;
    public bool IsInvincible() => isInvincible;
    
    /// <summary>
    /// Draws a red circle in the editor to visualize the ground check point.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
