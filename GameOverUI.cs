using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the game over panel and restart button functionality.
/// Shows the game over screen when the player dies and handles restarting.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;   // The panel that appears when game ends
    public Button restartButton;       // The button that restarts the game
    
    void Start()
    {
        // Make sure the game over panel is hidden at the start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Set up the restart button to call RestartGame when clicked
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("GameOverUI: Restart button initialized");
        }
        else
        {
            Debug.LogError("Restart button not assigned in GameOverUI!");
        }
    }
    
    /// <summary>
    /// Shows the game over panel (called by ScoreManager when player dies).
    /// </summary>
    public void ShowGameOver()
    {
        Debug.Log("Showing game over panel");
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    
    /// <summary>
    /// Restarts the game by reloading the scene.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("RestartGame called from GameOverUI - reloading scene");
        
        // Play button click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Unpause and reload
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
