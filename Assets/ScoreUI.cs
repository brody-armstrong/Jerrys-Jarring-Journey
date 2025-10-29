using UnityEngine;
using TMPro; // Using TextMeshPro for better text rendering

/// <summary>
/// Displays score, high score, distance, and combo information.
/// Clean, readable UI for gameplay.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the ScoreManager")]
    public ScoreManager scoreManager;
    
    [Tooltip("Reference to the PlayerController for distance tracking")]
    public PlayerController player;
    
    [Header("UI Text Elements (TextMeshPro)")]
    [Tooltip("Text element for current score display")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("Text element for high score display")]
    public TextMeshProUGUI highScoreText;
    
    [Tooltip("Text element for combo display (optional)")]
    public TextMeshProUGUI comboText;
    
    [Header("Visual Settings")]
    [Tooltip("Color for normal score text")]
    public Color normalColor = Color.white;
    
    [Tooltip("Color for new high score")]
    public Color highScoreColor = Color.yellow;
    
    [Tooltip("Color for combo text")]
    public Color comboColor = Color.cyan;
    
    [Tooltip("Flash duration when new high score is reached")]
    public float flashDuration = 0.2f;
    
    private bool isNewHighScore = false;
    private float flashTimer = 0f;
    
    void Start()
    {
        // Auto-find references if not assigned
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
        
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        // Initialize text colors
        if (scoreText != null) scoreText.color = normalColor;
        if (highScoreText != null) highScoreText.color = normalColor;
        if (comboText != null) comboText.color = comboColor;
    }
    
    void Update()
    {
        UpdateScoreDisplay();
        UpdateHighScoreDisplay();
        UpdateComboDisplay();
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText == null || scoreManager == null) return;
        
        scoreText.text = $"SCORE: {scoreManager.GetScoreText()}";
        
        // Check for new high score
        if (scoreManager.CurrentScore > scoreManager.HighScore && !isNewHighScore)
        {
            isNewHighScore = true;
            Debug.Log("🎉 NEW HIGH SCORE!");
        }
        
        // Flash effect for high score
        if (isNewHighScore)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer < flashDuration)
            {
                scoreText.color = highScoreColor;
            }
            else if (flashTimer < flashDuration * 2f)
            {
                scoreText.color = normalColor;
            }
            else
            {
                flashTimer = 0f; // Reset for continuous flash
            }
        }
    }
    
    void UpdateHighScoreDisplay()
    {
        if (highScoreText == null || scoreManager == null) return;
        
        highScoreText.text = $"HIGH SCORE: {scoreManager.GetHighScoreText()}";
    }
    
    void UpdateComboDisplay()
    {
        if (comboText == null || scoreManager == null) return;
        
        string comboString = scoreManager.GetComboText();
        
        if (!string.IsNullOrEmpty(comboString))
        {
            comboText.text = comboString;
            comboText.enabled = true;
            
            // Pulse effect for combo (optional enhancement)
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
            comboText.transform.localScale = Vector3.one * pulse;
        }
        else
        {
            comboText.enabled = false;
        }
    }
    
}

