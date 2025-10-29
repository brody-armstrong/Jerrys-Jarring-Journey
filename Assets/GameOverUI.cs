using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Game Over screen - shows score, high score, and retry/menu options
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    
    [Header("Scene Names")]
    public string titleSceneName = "TitleScene";
    public string gameplaySceneName = "SampleScene";
    
    private ScoreManager scoreManager;
    
    void Start()
    {
        // Find score manager
        scoreManager = FindObjectOfType<ScoreManager>();
        
        // Hide game over panel at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        
        // Show panel
        gameOverPanel.SetActive(true);
        
        // Update scores
        if (scoreManager != null)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {scoreManager.CurrentScore:F0}";
            }
            
            if (highScoreText != null)
            {
                highScoreText.text = $"HIGH SCORE: {scoreManager.HighScore:F0}";
            }
        }
        
        // Pause game
        Time.timeScale = 0f;
        
        Debug.Log("💀 GAME OVER! Showing game over screen.");
    }
    
    public void TryAgain()
    {
        // Play button click sound
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
        
        // Unpause
        Time.timeScale = 1f;
        
        // Go back to title screen
        SceneManager.LoadScene(titleSceneName);
    }
}

