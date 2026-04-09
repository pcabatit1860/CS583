using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script manages the animal parts that are attached to the player.
/// It handles equipping, removing, and visually attaching animal parts.
/// Only ONE animal part can be equipped at a time (the newest replaces the oldest).
/// </summary>
public class AnimalAssembler : MonoBehaviour
{
    [Header("Attachment Points")]
    public Transform backPoint;       // Where wings and shell attach (on the back)
    public Transform leftFootPoint;   // Where left shoe attaches
    public Transform rightFootPoint;  // Where right shoe attaches
    
    [Header("Visual Prefabs")]
    public GameObject batWingsPrefab;     // The bat wings model/sprite
    public GameObject cheetahShoePrefab;  // The cheetah shoe model/sprite
    public GameObject turtleShellPrefab;  // The turtle shell model/sprite
    
    [Header("Settings")]
    public bool dropOldPart = false;   // Should old parts fall to the ground when replaced?
    public float partDuration = 15f;   // How long bat wings and cheetah shoes last (turtle shell is permanent)
    
    // Current state
    private AnimalType currentAnimal = AnimalType.None;  // Which animal part is currently equipped
    private List<GameObject> currentVisuals = new List<GameObject>();  // The visual objects attached to the player
    private PlayerCore playerCore;     // Reference to the player's core script
    
    // Duration timer (only for Bat and Cheetah, not Turtle)
    private float partTimer = 0f;
    private bool hasActivePart = false;
    
    void Start()
    {
        playerCore = GetComponent<PlayerCore>();
        
        if (playerCore == null)
        {
            Debug.LogError("AnimalAssembler: PlayerCore component not found!");
        }
    }
    
    void Update()
    {
        // Only count down timer if we have an active part
        if (hasActivePart && currentAnimal != AnimalType.None)
        {
            // Turtle shell never expires (it's permanent until hit)
            if (currentAnimal == AnimalType.Turtle)
            {
                return;  // Skip timer for turtle shell
            }
            
            // Count down the timer for Bat and Cheetah parts
            partTimer += Time.deltaTime;
            if (partTimer >= partDuration)
            {
                Debug.Log($"⏰ {currentAnimal} part expired after {partDuration} seconds!");
                RemoveCurrentAnimal();  // Remove the expired part
                
                // Tell the spawner it can start spawning again
                TestSpawner spawner = FindObjectOfType<TestSpawner>();
                if (spawner != null)
                {
                    spawner.ResetSpawning();
                }
            }
        }
    }
    
    void LateUpdate()
    {
        // For Cheetah shoes, we don't want to force parent both shoes to the same point
        if (currentAnimal == AnimalType.Cheetah)
        {
            // Just make sure they're positioned correctly, but don't reparent
            foreach (GameObject visual in currentVisuals)
            {
                if (visual == null) continue;
                
                // Ensure position is zero relative to parent
                if (visual.transform.localPosition != Vector3.zero)
                {
                    visual.transform.localPosition = Vector3.zero;
                }
                
                // Ensure rotation is correct
                visual.transform.localRotation = Quaternion.identity;
            }
            return;
        }
        
        // For Bat and Turtle, force attachment to the correct point
        foreach (GameObject visual in currentVisuals)
        {
            if (visual == null) continue;
            
            Transform correctParent = GetCorrectParentForCurrentAnimal();
            if (correctParent == null) continue;
            
            // If the visual has the wrong parent, reattach it
            if (visual.transform.parent != correctParent)
            {
                visual.transform.SetParent(correctParent);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = GetCorrectRotationForCurrentAnimal();
                Debug.Log($"Force-reattached {visual.name} to {correctParent.name}");
            }
            
            // Ensure position is zero every frame
            if (visual.transform.localPosition != Vector3.zero)
            {
                visual.transform.localPosition = Vector3.zero;
            }
            
            // Ensure rotation is correct
            Quaternion targetRotation = GetCorrectRotationForCurrentAnimal();
            if (visual.transform.localRotation != targetRotation)
            {
                visual.transform.localRotation = targetRotation;
            }
        }
    }
    
    /// <summary>
    /// Called when the player collects an animal part.
    /// Replaces the current part with the new one.
    /// </summary>
    public void AddAnimalPart(AnimalPart part)
    {
        Debug.Log($"AnimalAssembler received: {part.animalName} (Type: {part.animalType})");
        
        // Remove whatever animal part we currently have
        RemoveCurrentAnimal();
        
        // Equip the new animal part
        currentAnimal = part.animalType;
        hasActivePart = true;
        partTimer = 0f;
        
        // Attach the visual model to the player
        AttachPartVisual(part);
        
        // Give the player the ability associated with this animal
        ApplyAbility(part.animalType);
        
        // Different message for turtle shell (permanent) vs others (temporary)
        if (currentAnimal == AnimalType.Turtle)
        {
            Debug.Log($"<color=green>✅ Equipped: {part.animalName}! (Permanent until hit)</color>");
        }
        else
        {
            Debug.Log($"<color=green>✅ Equipped: {part.animalName}! Will last {partDuration} seconds</color>");
        }
    }
    
    /// <summary>
    /// Removes the currently equipped animal part (visual and ability).
    /// </summary>
    void RemoveCurrentAnimal()
    {
        if (currentAnimal == AnimalType.None) return;
        
        Debug.Log($"Removing current animal: {currentAnimal}");
        hasActivePart = false;
        partTimer = 0f;
        
        // Destroy all visual objects attached to the player
        foreach (GameObject visual in currentVisuals)
        {
            if (visual != null)
            {
                if (dropOldPart)
                {
                    // Make the part fall to the ground
                    visual.transform.parent = null;
                    Rigidbody2D rb = visual.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 1f;
                    rb.velocity = new Vector2(Random.Range(-3f, -1f), Random.Range(2f, 4f));
                    Destroy(visual, 5f);
                }
                else
                {
                    Destroy(visual);
                }
            }
        }
        currentVisuals.Clear();
        
        // Remove the ability from the player
        if (playerCore != null)
        {
            playerCore.DisableAllAbilities();
        }
        
        currentAnimal = AnimalType.None;
    }
    
    /// <summary>
    /// Gives the player the ability associated with this animal type.
    /// </summary>
    void ApplyAbility(AnimalType type)
    {
        if (playerCore == null) return;
        
        switch (type)
        {
            case AnimalType.Bat:
                playerCore.EnableBatWings();
                break;
            case AnimalType.Cheetah:
                playerCore.EnableCheetahSpeed();
                break;
            case AnimalType.Turtle:
                playerCore.EnableTurtleShell(1);
                break;
        }
    }
    
    /// <summary>
    /// Attaches the visual model of the animal part to the player.
    /// </summary>
    void AttachPartVisual(AnimalPart part)
    {
        switch (part.animalType)
        {
            case AnimalType.Bat:
                if (batWingsPrefab != null && backPoint != null)
                {
                    GameObject visual = Instantiate(batWingsPrefab);
                    visual.transform.SetParent(backPoint);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    currentVisuals.Add(visual);
                    Debug.Log($"Bat wings attached to {backPoint.name}");
                }
                break;
                
            case AnimalType.Cheetah:
                if (cheetahShoePrefab != null)
                {
                    Debug.Log("Spawning Cheetah shoes...");
                    
                    // LEFT SHOE (player's left foot = screen right)
                    // Note: We attach to rightFootPoint because of screen orientation
                    if (rightFootPoint != null)
                    {
                        GameObject leftShoe = Instantiate(cheetahShoePrefab);
                        leftShoe.transform.SetParent(rightFootPoint);
                        leftShoe.transform.localPosition = Vector3.zero;
                        leftShoe.transform.localRotation = Quaternion.identity;
                        
                        // Make sure shoe renders on top of other sprites
                        SpriteRenderer sr = leftShoe.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sortingOrder = 10;
                        }
                        
                       
