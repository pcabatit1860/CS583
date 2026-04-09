using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the overall game state - whether the game is active, game over, etc.
/// Uses a singleton pattern so it can be accessed from anywhere.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool isGameActive = false;   // Is gameplay currently happening?
    public bool isGameOver = false;     // Has the player died?
    
    private PlayerCore player;
    private TestSpawner spawner;
    
    void Awake()
    {
        // Singleton pattern - only one GameManager should exist
        // This also survives scene reloads (DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep this object when loading new scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find references to important game objects
        player = FindObjectOfType<PlayerCore>();
        spawner = FindObjectOfType<TestSpawner>();
        
        // Initially, game is waiting for title screen
        isGameActive = false;
        isGameOver = false;
    }
    
    /// <summary>
    /// Starts the game - enables player and spawner, unpauses time.
    /// </summary>
    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (player != null)
            player.enabled = true;
        if (spawner != null)
            spawner.enabled = true;
    }
    
    /// <summary>
    /// Ends the game - disables player and spawner, pauses time.
    /// </summary>
    public void GameOver()
    {
        isGameActive = false;
        isGameOver = true;
        Time.timeScale = 0f;
        
        if (player != null)
            player.enabled = false;
        if (spawner != null)
            spawner.enabled = false;
    }
    
    /// <summary>
    /// Reloads the current scene to restart the game.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
