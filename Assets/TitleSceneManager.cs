using UnityEngine;
using TMPro; // TextMeshPro support
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the title/intro screen.
/// Shows game title, high score, and "Press SPACE to Start" prompt.
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Title text (e.g. 'Jerry's Jarring Journey')")]
    public TextMeshProUGUI titleText;
    
    [Tooltip("High score display text")]
    public TextMeshProUGUI highScoreText;
    
    [Tooltip("'Press SPACE to Start' text")]
    public TextMeshProUGUI promptText;
    
    [Header("Settings")]
    [Tooltip("Name of the gameplay scene to load")]
    public string gameplaySceneName = "GameplayScene";
    
    [Tooltip("Prompt pulse speed")]
    public float pulseSpeed = 2f;
    
    [Tooltip("Prompt pulse amount (0-1)")]
    public float pulseAmount = 0.3f;
    
    private bool isStarting = false;
    private float pulseTimer = 0f;
    
    void Start()
    {
        // Load and display high score
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"HIGH SCORE: {highScore}";
        }
        
        // Set title
        if (titleText != null)
        {
            titleText.text = "Jerry's Jarring Journey";
        }
    }
    
    void Update()
    {
        if (isStarting) return;
        
        // Pulse the "Press SPACE" prompt
        if (promptText != null)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float alpha = 1f - (Mathf.Sin(pulseTimer) * pulseAmount);
            Color color = promptText.color;
            promptText.color = new Color(color.r, color.g, color.b, alpha);
        }
        
        // Check for space input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }
    
    void StartGame()
    {
        isStarting = true;
        
        // Load gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }
}

