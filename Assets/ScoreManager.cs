using UnityEngine;

/// <summary>
/// Manages scoring system with distance, airtime, and combo multipliers.
/// Tracks high scores and provides real-time score feedback.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player controller")]
    public PlayerController player;
    
    [Header("Score Multipliers")]
    [Tooltip("Base points per meter traveled")]
    public float pointsPerMeter = 10f;
    
    [Tooltip("Bonus points per second of airtime")]
    public float airtimePointsPerSecond = 50f;
    
    [Tooltip("Multiplier for consecutive good landings")]
    public float comboMultiplier = 1.2f;
    
    [Tooltip("Maximum combo multiplier (caps at this value)")]
    public float maxComboMultiplier = 3.0f;
    
    [Header("Combo System")]
    [Tooltip("Minimum airtime to count towards combo (seconds)")]
    public float minAirtimeForCombo = 0.5f;
    
    [Tooltip("Time window to maintain combo after landing (seconds)")]
    public float comboTimeWindow = 2f;
    
    [Header("High Score")]
    [Tooltip("PlayerPrefs key for high score storage")]
    public string highScoreKey = "JerryHighScore";
    
    [Header("Debug Display")]
    [Tooltip("Show score on screen using OnGUI (no UI setup needed)")]
    public bool showDebugDisplay = false; // DISABLED - using ScoreUI instead
    
    // Score tracking
    private float currentScore = 0f;
    private float highScore = 0f;
    private int currentCombo = 0;
    private float currentComboMultiplier = 1f;
    
    // Airtime tracking
    private float currentAirtime = 0f;
    private bool wasGrounded = true;
    private float lastLandingTime = 0f;
    
    // Public accessors
    public float CurrentScore => currentScore;
    public float HighScore => highScore;
    public int CurrentCombo => currentCombo;
    public float CurrentComboMultiplier => currentComboMultiplier;
    public float CurrentAirtime => currentAirtime;
    
    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        // Load high score
        highScore = PlayerPrefs.GetFloat(highScoreKey, 0f);
    }
    
    void Update()
    {
        if (player == null || !player.isAlive) return;
        
        // Track airtime
        UpdateAirtime();
        
        // Calculate score
        UpdateScore();
        
        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetFloat(highScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }
    
    void UpdateAirtime()
    {
        // Check if player is grounded using reflection or public property
        // Assuming we can access isGrounded through a raycast similar to the player
        bool isGrounded = CheckIfPlayerGrounded();
        
        // Track airtime
        if (!isGrounded)
        {
            currentAirtime += Time.deltaTime;
            
            // Entering air
            if (wasGrounded)
            {
                wasGrounded = false;
            }
        }
        else
        {
            // Landing
            if (!wasGrounded)
            {
                HandleLanding();
                wasGrounded = true;
                currentAirtime = 0f;
            }
        }
    }
    
    void HandleLanding()
    {
        // Check if airtime qualifies for combo
        if (currentAirtime >= minAirtimeForCombo)
        {
            // Award airtime bonus with current combo multiplier
            float airtimeBonus = currentAirtime * airtimePointsPerSecond * currentComboMultiplier;
            currentScore += airtimeBonus;
            
            // Increment combo
            currentCombo++;
            currentComboMultiplier = Mathf.Min(
                1f + (currentCombo * (comboMultiplier - 1f)),
                maxComboMultiplier
            );
            
            lastLandingTime = Time.time;
        }
        else
        {
            // Reset combo if airtime too short or too long since last landing
            if (Time.time - lastLandingTime > comboTimeWindow)
            {
                currentCombo = 0;
                currentComboMultiplier = 1f;
            }
        }
    }
    
    void UpdateScore()
    {
        // Distance-based score (always accumulating)
        float distanceScore = player.distanceTravelled * pointsPerMeter;
        
        // Base score is just distance traveled
        // Bonuses are added in HandleLanding()
        currentScore = distanceScore;
    }
    
    bool CheckIfPlayerGrounded()
    {
        // Simple raycast to check if player is grounded
        // Using same logic as PlayerController
        if (player == null) return false;
        
        LayerMask groundLayer = LayerMask.GetMask("Ground", "Default");
        RaycastHit2D hit = Physics2D.Raycast(
            player.transform.position,
            Vector2.down,
            0.6f, // Same as PlayerController.groundCheckDistance
            groundLayer
        );
        
        return hit.collider != null;
    }
    
    /// <summary>
    /// Reset score for a new game
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0f;
        currentCombo = 0;
        currentComboMultiplier = 1f;
        currentAirtime = 0f;
        lastLandingTime = 0f;
    }
    
    /// <summary>
    /// Get formatted score string
    /// </summary>
    public string GetScoreText()
    {
        return currentScore.ToString("F0");
    }
    
    /// <summary>
    /// Get formatted high score string
    /// </summary>
    public string GetHighScoreText()
    {
        return highScore.ToString("F0");
    }
    
    /// <summary>
    /// Get formatted combo string
    /// </summary>
    public string GetComboText()
    {
        if (currentCombo > 0)
        {
            return $"x{currentCombo} COMBO! (x{currentComboMultiplier:F1})";
        }
        return "";
    }
    
    /// <summary>
    /// Debug on-screen display (no UI setup needed!)
    /// </summary>
    void OnGUI()
    {
        if (!showDebugDisplay || player == null) return;
        
        // Create a simple on-screen display
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        
        // Draw score info in top-left corner
        GUI.Label(new Rect(10, 10, 400, 30), $"SCORE: {currentScore:F0}", style);
        GUI.Label(new Rect(10, 35, 400, 30), $"DISTANCE: {player.distanceTravelled:F0}m", style);
        GUI.Label(new Rect(10, 60, 400, 30), $"HIGH SCORE: {highScore:F0}", style);
        
        // Draw combo if active
        if (currentCombo > 0)
        {
            GUIStyle comboStyle = new GUIStyle();
            comboStyle.fontSize = 28;
            comboStyle.normal.textColor = Color.cyan;
            comboStyle.fontStyle = FontStyle.Bold;
            comboStyle.alignment = TextAnchor.UpperCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 40), 
                     $"x{currentCombo} COMBO! (x{currentComboMultiplier:F1})", comboStyle);
        }
        
        // New high score indicator
        if (currentScore > highScore && currentScore > 0)
        {
            GUIStyle highScoreStyle = new GUIStyle();
            highScoreStyle.fontSize = 24;
            highScoreStyle.normal.textColor = Color.yellow;
            highScoreStyle.fontStyle = FontStyle.Bold;
            highScoreStyle.alignment = TextAnchor.UpperRight;
            
            GUI.Label(new Rect(Screen.width - 300, 10, 290, 30), 
                     "🎉 NEW HIGH SCORE!", highScoreStyle);
        }
    }
}

