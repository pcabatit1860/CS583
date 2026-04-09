using UnityEngine;

/// <summary>
/// Central manager for all audio in the game - sound effects and background music.
/// Uses singleton pattern so it can be accessed from any script.
/// Persists across scene loads with DontDestroyOnLoad.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Sound Effects")]
    public AudioClip buttonClickSound;     // Played when pressing Play or Restart
    public AudioClip jumpSound;            // Played when player jumps
    public AudioClip collectSound;         // Played when collecting an animal part
    public AudioClip obstacleHitSound;     // Played when hitting obstacle without shell (game over)
    public AudioClip turtleShellHitSound;  // Played when turtle shell breaks from hit
    
    [Header("Background Music")]
    public AudioClip titleMusic;           // Music on the title screen
    public AudioClip gameOverMusic;        // Music when game over screen appears
    
    [Header("Audio Sources")]
    public AudioSource sfxSource;     // For sound effects (short sounds)
    public AudioSource musicSource;   // For background music (looping)
    
    void Awake()
    {
        // Singleton pattern - only one AudioManager should exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep this object when loading new scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create audio sources if they weren't assigned in the Inspector
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;  // Music loops by default
        }
    }
    
    // ========== SOUND EFFECT METHODS ==========
    // These play short, non-looping sounds
    
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSound);
        else
            Debug.LogWarning("Button click sound missing!");
    }
    
    public void PlayJump()
    {
        if (jumpSound != null && sfxSource != null)
            sfxSource.PlayOneShot(jumpSound);
        else
            Debug.LogWarning("Jump sound missing!");
    }
    
    public void PlayCollect()
    {
        if (collectSound != null && sfxSource != null)
            sfxSource.PlayOneShot(collectSound);
        else
            Debug.LogWarning("Collect sound missing!");
    }
    
    public void PlayObstacleHit()
    {
        if (obstacleHitSound != null && sfxSource != null)
            sfxSource.PlayOneShot(obstacleHitSound);
        else
            Debug.LogWarning("Obstacle hit sound missing!");
    }
    
    public void PlayTurtleShellHit()
    {
        if (turtleShellHitSound != null && sfxSource != null)
            sfxSource.PlayOneShot(turtleShellHitSound);
        else
            Debug.LogWarning("Turtle shell hit sound missing!");
    }
    
    // ========== MUSIC METHODS ==========
    // These handle background music
    
    public void PlayTitleMusic()
    {
        if (titleMusic != null && musicSource != null)
        {
            musicSource.clip = titleMusic;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("Playing title music");
        }
        else
            Debug.LogWarning("Title music missing!");
    }
    
    public void PlayGameOverMusic()
    {
        if (gameOverMusic != null && musicSource != null)
        {
            musicSource.clip = gameOverMusic;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("Playing game over music");
        }
        else
            Debug.LogWarning("Game over music missing!");
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }
}
