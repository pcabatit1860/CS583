using UnityEngine;
using TMPro;  // Required for TextMeshPro

/// <summary>
/// This script applies your custom font to all TextMeshPro text in the game.
/// Makes the game look consistent with your chosen font style.
/// </summary>
public class FontManager : MonoBehaviour
{
    public static FontManager Instance;
    public TMP_FontAsset customFont;  // Drag your custom font asset here in the Inspector
    
    void Awake()
    {
        // Singleton pattern - only one FontManager should exist
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Apply the custom font to all text in the scene
        ApplyFontToAllText();
    }
    
    /// <summary>
    /// Finds every TextMeshPro text object in the scene and changes its font.
    /// </summary>
    public void ApplyFontToAllText()
    {
        if (customFont == null)
        {
            Debug.LogWarning("Custom font not assigned to FontManager! Please drag your font asset into the Custom Font field.");
            return;
        }
        
        // Find all TextMeshPro objects (including inactive ones)
        TMP_Text[] allText = FindObjectsOfType<TMP_Text>(true);
        
        // Change each one to use our custom font
        foreach (TMP_Text text in allText)
        {
            text.font = customFont;
        }
        
        Debug.Log($"Applied custom font to {allText.Length} text objects");
    }
}
