using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JERRY'S JARRING JOURNEY - Avalanche Controller
/// 
/// Controls the avalanche that chases Jerry.
/// 
/// Key Features:
/// - Starts visible on screen at game start
/// - Slow creep (speed 2-15) for dramatic effect
/// - Max distance cap (15 units) - teleports if too far behind
/// - Tutorial grace period (7 seconds - won't catch player)
/// - Dynamic speed adjustment based on player performance
/// - Camera shake when visible (optional)
/// </summary>
public class AvalancheController : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Reference to Jerry (PlayerController component)")]
    public PlayerController player;
    
    [Header("Speed Settings - DEMO TUNED (SLOW CREEP!)")]
    [Tooltip("Starting speed of avalanche")]
    public float baseSpeed = 2f; // SLOW CREEP - visible approach!
    
    [Tooltip("Acceleration per second")]
    public float acceleration = 0.2f; // Very slow acceleration
    
    [Tooltip("Maximum avalanche speed cap")]
    public float maxSpeed = 15f; // LOW cap - slow, menacing creep
    
    [Header("Gap Management - DEMO TUNED")]
    [Tooltip("Maximum allowed gap (teleports if exceeded)")]
    public float maxGap = 15f; // Appears on screen sooner!
    
    [Tooltip("Minimum gap to maintain (prevents instant death)")]
    public float minGap = 3f;
    
    [Tooltip("Time to catch idle player (seconds)")]
    public float timeToCatchIdlePlayer = 15f; // 15 seconds if Jerry doesn't move
    
    [Tooltip("Periodic speed increase interval (seconds)")]
    public float speedIncreaseInterval = 20f; // Speed up every 20 seconds
    
    [Tooltip("Speed increase amount per interval")]
    public float speedIncreaseAmount = 2f; // Bigger speed jumps
    
    [Header("Danger Zone")]
    [Tooltip("Distance at which danger feedback starts (screen shake, particles, etc.)")]
    public float dangerThreshold = 20f; // Shake when close!
    
    [Tooltip("Distance at which extreme danger feedback starts")]
    public float extremeDangerThreshold = 10f;
    
    [Header("Visual Feedback")]
    [Tooltip("Particle system for snow/mist effect when avalanche is close")]
    public ParticleSystem dangerParticles;
    
    [Tooltip("Camera shake intensity (screen shake when visible)")]
    public float shakeIntensity = 0.2f; // Increased for visibility!
    
    [Tooltip("Camera shake frequency (higher = faster shake)")]
    public float shakeFrequency = 25f;
    
    [Tooltip("Camera shake smoothness (lower = jerkier shake)")]
    public float shakeSmoothing = 8f;
    
    [Tooltip("Screen shake when avalanche is visible on screen")]
    public bool shakeWhenVisible = false; // DISABLED - using text warning instead
    
    [Tooltip("Manual shake distance threshold (if not using visibility check)")]
    public float shakeThreshold = 15f;
    
    [Header("Collision")]
    [Tooltip("Layer mask for detecting Jerry")]
    public LayerMask playerLayer;
    
    [Tooltip("Collision detection radius")]
    public float collisionRadius = 1.5f;
    
    [Tooltip("Tutorial grace period - don't catch player for this many seconds")]
    public float tutorialGracePeriod = 12f; // Longer grace period for full tutorial
    
    [Header("Starting Position")]
    [Tooltip("Use fixed starting position instead of relative to player")]
    public bool useFixedStartPosition = true;
    
    [Tooltip("Fixed starting position (e.g. -14, 0, 0)")]
    public Vector3 fixedStartPosition = new Vector3(-14f, 0f, 0f);
    
    [Tooltip("Starting distance behind the player (only used if useFixedStartPosition is false)")]
    public float startingDistance = 15f;
    
    [Header("Movement Style")]
    [Tooltip("How the avalanche moves: Horizontal only, or downward ramp")]
    public MovementStyle movementStyle = MovementStyle.Horizontal;
    
    [Tooltip("Downward slope angle (only used if movementStyle = DownwardRamp)")]
    public float downwardSlope = 0.2f;
    
    [Header("Terrain Following")]
    [Tooltip("Should avalanche follow player's Y position to stay with terrain?")]
    public bool followTerrainHeight = true;
    
    [Tooltip("Vertical offset from player's Y position")]
    public float terrainYOffset = 0f;
    
    [Tooltip("How smoothly avalanche adjusts to terrain height (0 = instant, higher = smoother)")]
    public float terrainFollowSmoothing = 5f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showGizmos = true;
    
    public enum MovementStyle
    {
        Horizontal,      // Move only horizontally (stays at same Y level)
        DownwardRamp    // Move forward and downward (like sliding down a ramp)
    }
    
    // Internal state
    private float currentSpeed;
    private float currentGap;
    private bool isInDangerZone;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private float gameTime = 0f; // Track time for time-based speed increases
    private Vector3 cameraShakeOffset = Vector3.zero;
    private float shakeTimer = 0f;
    
    void Start()
    {
        // Validate player reference
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        // Get camera reference for shake effect
        mainCamera = Camera.main;
        
        // Position avalanche at start
        if (useFixedStartPosition)
        {
            // Use fixed world position (e.g. -14, 0, 0)
            transform.position = fixedStartPosition;
        }
        else if (player != null)
        {
            // Position behind player (old behavior)
            Vector3 startPos = player.transform.position;
            startPos.x -= startingDistance;
            transform.position = startPos;
        }
        
        // Initialize speed
        currentSpeed = baseSpeed;
        
        // Disable danger particles at start
        if (dangerParticles != null)
        {
            dangerParticles.Stop();
        }
    }
    
    void Update()
    {
        if (player == null || !player.isAlive) return;
        
        // Track game time
        gameTime += Time.deltaTime;
        
        // Calculate current gap
        currentGap = player.transform.position.x - transform.position.x;
        
        // PERIODIC SPEED INCREASES (ramps up pressure over time)
        int speedIncreaseSteps = Mathf.FloorToInt(gameTime / speedIncreaseInterval);
        float periodicSpeedBonus = speedIncreaseSteps * speedIncreaseAmount;
        
        // GRADUAL ACCELERATION + PERIODIC BONUSES
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        currentSpeed = Mathf.Clamp(currentSpeed + periodicSpeedBonus, baseSpeed, maxSpeed);
        
        // TELEPORT if gap exceeds max (keeps avalanche close!)
        if (currentGap > maxGap)
        {
            Vector3 warpPos = player.transform.position;
            warpPos.x -= maxGap;
            transform.position = new Vector3(warpPos.x, transform.position.y, transform.position.z);
            currentGap = maxGap;
        }
        
        // DYNAMIC SPEED: Slow creep toward Jerry
        float effectiveSpeed = currentSpeed;
        
        // If Jerry is going slow/stopped, speed up VERY slightly
        float playerSpeed = player.GetComponent<Rigidbody2D>().velocity.x;
        if (playerSpeed < 3f) // Jerry is slow/stopped
        {
            // Catch up slowly - visible, menacing approach
            effectiveSpeed = Mathf.Max(effectiveSpeed, playerSpeed + 1.5f); // Just 1.5 units/sec faster
        }
        
        // Move avalanche based on movement style
        Vector3 moveDirection;
        if (movementStyle == MovementStyle.DownwardRamp)
        {
            // Move forward and downward (like sliding down a ramp)
            moveDirection = new Vector3(1f, -downwardSlope, 0f).normalized;
        }
        else
        {
            // Move only horizontally (stays at same Y level)
            moveDirection = Vector3.right;
        }
        
        transform.position += moveDirection * effectiveSpeed * Time.deltaTime;
        
        // Follow terrain height if enabled
        if (followTerrainHeight && player != null)
        {
            float targetY = player.transform.position.y + terrainYOffset;
            float currentY = transform.position.y;
            
            if (terrainFollowSmoothing > 0)
            {
                // Smooth transition to target height
                float newY = Mathf.Lerp(currentY, targetY, Time.deltaTime * terrainFollowSmoothing);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                // Instant snap to target height
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }
        
        // Check danger zones
        UpdateDangerFeedback();
        
        // Check collision
        CheckCollision();
    }
    
    void LateUpdate()
    {
        // Apply SCREEN SHAKE to camera (not the avalanche object!)
        // This runs AFTER all camera movement/following
        
        if (mainCamera != null && cameraShakeOffset.magnitude > 0.001f)
        {
            // Apply shake by temporarily offsetting camera
            mainCamera.transform.position += cameraShakeOffset;
        }
    }
    
    void UpdateDangerFeedback()
    {
        // Check if in danger zone
        bool wasInDangerZone = isInDangerZone;
        isInDangerZone = currentGap < dangerThreshold;
        
        // Activate danger particles when entering danger zone
        if (isInDangerZone && !wasInDangerZone)
        {
            if (dangerParticles != null && !dangerParticles.isPlaying)
            {
                dangerParticles.Play();
            }
        }
        else if (!isInDangerZone && wasInDangerZone)
        {
            if (dangerParticles != null && dangerParticles.isPlaying)
            {
                dangerParticles.Stop();
            }
        }
        
        // Screen shake when avalanche is visible on screen
        bool shouldShake = false;
        
        if (shakeWhenVisible && mainCamera != null)
        {
            // Check if avalanche is visible in camera viewport
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
            shouldShake = isVisible;
        }
        else
        {
            // Use distance threshold instead
            shouldShake = currentGap < shakeThreshold;
        }
        
        if (shouldShake && mainCamera != null)
        {
            ApplyCameraShake();
        }
        else
        {
            // Not shaking - smoothly reduce shake to zero
            cameraShakeOffset = Vector3.Lerp(cameraShakeOffset, Vector3.zero, Time.deltaTime * shakeSmoothing * 2f);
        }
    }
    
    void ApplyCameraShake()
    {
        if (mainCamera == null) return;
        
        // Calculate shake magnitude
        float shakeMagnitude = shakeIntensity;
        
        // If using distance-based shake instead of visibility, scale by proximity
        if (!shakeWhenVisible)
        {
            shakeMagnitude = shakeIntensity * (1f - (currentGap / shakeThreshold));
            shakeMagnitude = Mathf.Clamp01(shakeMagnitude);
        }
        
        // Use Perlin noise for smooth, natural shake
        shakeTimer += Time.deltaTime * shakeFrequency;
        
        float shakeX = (Mathf.PerlinNoise(shakeTimer, 0f) - 0.5f) * 2f * shakeMagnitude;
        float shakeY = (Mathf.PerlinNoise(0f, shakeTimer) - 0.5f) * 2f * shakeMagnitude;
        
        // Target shake offset
        Vector3 targetShake = new Vector3(shakeX, shakeY, 0f);
        
        // Smooth shake transition
        cameraShakeOffset = Vector3.Lerp(cameraShakeOffset, targetShake, Time.deltaTime * shakeSmoothing);
    }
    
    void CheckCollision()
    {
        // TUTORIAL GRACE PERIOD - Don't catch player during first few seconds!
        if (gameTime < tutorialGracePeriod)
        {
            return; // Skip collision check during tutorial
        }
        
        // Check if avalanche has caught Jerry
        if (currentGap <= 0f || currentGap < minGap * 0.5f)
        {
            // Caught!
            CatchPlayer();
        }
    }
    
    void CatchPlayer()
    {
        if (player == null || !player.isAlive) return;
        
        // Kill the player
        player.CaughtByAvalanche();
        
        // Stop danger particles
        if (dangerParticles != null)
        {
            dangerParticles.Stop();
        }
        
        // Play death sound
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayDeathSound();
        }
        
        // Show game over screen
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw avalanche position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
        
        // Draw danger zones if player exists
        if (player != null)
        {
            // Danger threshold
            Gizmos.color = Color.yellow;
            Vector3 dangerPos = transform.position + Vector3.right * dangerThreshold;
            Gizmos.DrawLine(dangerPos + Vector3.up * 5f, dangerPos + Vector3.down * 5f);
            
            // Extreme danger threshold
            Gizmos.color = Color.red;
            Vector3 extremeDangerPos = transform.position + Vector3.right * extremeDangerThreshold;
            Gizmos.DrawLine(extremeDangerPos + Vector3.up * 5f, extremeDangerPos + Vector3.down * 5f);
            
            // Max gap line
            Gizmos.color = Color.green;
            Vector3 maxGapPos = transform.position + Vector3.right * maxGap;
            Gizmos.DrawLine(maxGapPos + Vector3.up * 5f, maxGapPos + Vector3.down * 5f);
            
            // Draw line to player
            Gizmos.color = isInDangerZone ? Color.red : Color.white;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
}

