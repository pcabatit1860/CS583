using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the title screen UI, music, and game start.
/// When the game launches, this script pauses the game and shows the title panel.
/// Pressing the Play button starts the actual gameplay.
/// </summary>
public class TitleScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject titlePanel;     // The panel containing title text, description, and play button
    public GameObject gameUIPanel;    // The panel that shows score during gameplay
    public Button playButton;         // The button that starts the game
    
    void Start()
    {
        // Set up the play button to call StartGame when clicked
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
        }
        
        // Show the title screen, hide the game UI
        if (titlePanel != null)
            titlePanel.SetActive(true);
        if (gameUIPanel != null)
            gameUIPanel.SetActive(false);
        
        // Start playing the title screen music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTitleMusic();
        
        // Pause the game until the player presses Play
        Time.timeScale = 0f;
        
        Debug.Log("Title screen initialized - game paused");
    }
    
    /// <summary>
    /// Called when the player clicks the Play button.
    /// Hides the title screen, shows the game UI, and starts the game.
    /// </summary>
    void StartGame()
    {
        Debug.Log("StartGame called");
        
        // Play button click sound effect
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Hide the title panel completely
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
            Debug.Log("Title panel hidden");
        }
        
        // Show the game UI (score displays)
        if (gameUIPanel != null)
            gameUIPanel.SetActive(true);
        
        // Resume the game (unpause)
        Time.timeScale = 1f;
        
        // Stop the title music (gameplay music will start from other scripts if needed)
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
        
        Debug.Log("Game started");
    }
}
