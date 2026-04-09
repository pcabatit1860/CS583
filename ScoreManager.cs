using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the score system, high score saving, and game over UI.
/// Score increases based on distance traveled and animal parts collected.
/// High scores are saved between game sessions using PlayerPrefs.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;       // Current score display
    public TextMeshProUGUI highScoreText;   // High score display
    public TextMeshProUGUI finalScoreText;  // Final score on game over screen
    public GameObject gameOverPanel;        // Panel shown when player dies
    public Button restartButton;            // Button to restart the game
    
    [Header("Score Settings")]
    public float distanceMultiplier = 10f;  // Points per unit traveled
    
    private int currentScore = 0;
    private int highScore = 0;
    private Transform player;
    private float startX;      // Player's X position at game start
    private float highestX;    // Furthest X position the player has reached
    private bool isGameActive = true;
    
    void Awake()
    {
        // Singleton pattern - only one ScoreManager
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        if (player != null)
        {
            startX = player.position.x;
            highestX = startX;
        }
        
        // Load saved high score from previous play sessions
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Update the UI displays
        UpdateScoreUI();
        UpdateHighScoreUI();
        
        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Set up the restart button
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("ScoreManager: Restart button initialized");
        }
    }
    
    void Update()
    {
        if (!isGameActive) return;
        if (player == null) return;
        
        // Track the furthest distance reached
        if (player.position.x > highestX)
        {
            highestX = player.position.x;
            float distanceRun = highestX - startX;
            currentScore = Mathf.FloorToInt(distanceRun * distanceMultiplier);
            
            UpdateScoreUI();
            
            // Check if we beat the high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
                UpdateHighScoreUI();
            }
        }
    }
    
    /// <summary>
    /// Adds bonus points for collecting animal parts.
    /// </summary>
    public void AddCollectionBonus(AnimalType animalType)
    {
        if (!isGameActive) return;
        
        int bonus = 0;
        switch (animalType)
        {
            case AnimalType.Bat:
                bonus = 100;
                break;
            case AnimalType.Cheetah:
                bonus = 150;
                break;
            case AnimalType.Turtle:
                bonus = 200;
                break;
        }
        
        currentScore += bonus;
        UpdateScoreUI();
        
        // Check if the bonus pushed us over the high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            UpdateHighScoreUI();
        }
        
        Debug.Log($"🎯 +{bonus} points! Total: {currentScore}");
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }
    
    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore;
    }
    
    /// <summary>
    /// Called when the player dies. Shows game over panel, plays sounds, saves high score.
    /// </summary>
    public void GameOver()
    {
        isGameActive = false;
    
        // Save the high score
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    
        // Play obstacle hit sound effect
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayObstacleHit();
    
        // Play game over music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOverMusic();
    
        // Show the game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = "Score: " + currentScore + "\nBest: " + highScore;
        }
    
        // Disable player movement
        PlayerCore playerCore = FindObjectOfType<PlayerCore>();
        if (playerCore != null)
            playerCore.enabled = false;
    
        // Disable the spawner
        TestSpawner spawner = FindObjectOfType<TestSpawner>();
        if (spawner != null)
            spawner.enabled = false;
    
        // IMPORTANT: Do NOT call StopMusic() here - it would stop the game over music!
    
        // Pause the game
        Time.timeScale = 0f;
    
        Debug.Log("Game Over! Final Score: " + currentScore + ", Best: " + highScore);
    }
    
    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("RestartGame called from ScoreManager");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Getter methods for other scripts
    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;
}
