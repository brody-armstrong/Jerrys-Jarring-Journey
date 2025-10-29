using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    [Header("Sprite Swapping")]
    [Tooltip("Sprite to show when NOT tucking (standing/normal pose)")]
    public Sprite standingSprite;
    
    [Tooltip("Sprite to show when tucking (crouched pose)")]
    public Sprite tuckedSprite;
    
    [Header("Speed Settings")]
    public float minSpeed = 2f;
    public float baseSpeedCap = 25f; // Natural speed cap without skill
    public float maxSpeed = 35f; // Only reachable via skillful launches
    private float currentSpeed = 15f; // Start at comfortable speed
    
    [Tooltip("Speed change smoothing (higher = smoother transitions, less jittery)")]
    public float speedSmoothTime = 0.15f; // Smooth speed changes over ~150ms
    private float targetSpeed = 15f; // Target speed we're smoothing toward
    private float speedVelocity = 0f; // SmoothDamp velocity
    
    [Header("Tuck & Release Mechanic")]
    [Tooltip("Base gravity acceleration on slopes (g * sin(θ) multiplier)")]
    public float gravityAccelMultiplier = 50f; // BALANCED for smooth speed changes
    
    [Tooltip("Additional acceleration boost when tucking on downhill")]
    public float tuckAccelMultiplier = 1.5f; // Moderate boost, not extreme
    
    [Tooltip("Drag when tucking (higher = more friction tradeoff)")]
    public float tuckDragCoefficient = 1.8f; // Moderate drag for smooth feel
    
    [Tooltip("Drag when NOT tucking (lower = coast easier)")]
    public float normalDragCoefficient = 0.9f; // Gentle passive slowdown
    
    [Tooltip("Uphill penalty multiplier when tucking")]
    public float uphillPenaltyMultiplier = 3.0f; // Noticeable but not extreme
    
    [Tooltip("Speed-proportional drag coefficient (creates natural speed ceiling)")]
    public float speedProportionalDrag = 0.005f; // Gentler ceiling
    
    [Header("Launch Conversion")]
    [Tooltip("How much tangential speed converts to upward launch (0.4 = 40%)")]
    public float speedToLaunchConversion = 0.4f;
    
    [Tooltip("Minimum base launch velocity")]
    public float baseLaunchVelocity = 10f;
    
    [Tooltip("Release boost multiplier (extra pop on release)")]
    public float releaseBoostMultiplier = 1.2f;
    
    [Tooltip("Maximum upward launch velocity cap")]
    public float maxLaunchVelocity = 50f;
    
    [Header("Uphill Braking")]
    [Tooltip("Braking force when tucking on uphill (penalty for bad timing)")]
    public float uphillBrakingForce = 40f;
    
    [Tooltip("Uphill friction multiplier")]
    public float uphillDragMultiplier = 3.0f;
    
    [Header("Ground Stick (Critical for Tuck Mechanic)")]
    [Tooltip("Downward force to keep Jerry on terrain when tucking (lower = smoother feel)")]
    public float groundStickForce = 120f; // Reduced for smoother transitions
    
    [Tooltip("Downward force when tucking in air (pulls back to ground)")]
    public float tuckAirDownForce = 180f; // Reduced for smoother feel
    
    [Tooltip("Gravity multiplier when tucking in air")]
    public float tuckGravityMultiplier = 1.8f;
    
    [Header("Air Drag System")]
    [Tooltip("Base air drag coefficient")]
    public float airDrag = 0.995f;
    
    [Tooltip("Speed-dependent air drag (higher speed = more drag)")]
    public float speedBasedAirDrag = 0.001f;
    
    [Tooltip("Reduced drag when releasing tuck in air (floaty feel)")]
    public float floatyDragMultiplier = 0.998f; // Higher = less drag = floatier
    
    [Tooltip("Frames after releasing tuck where floaty physics apply")]
    public int floatyFrames = 15;
    
    [Header("Landing Penalty System")]
    [Tooltip("Energy loss factor for bad landings")]
    public float landingLossFactor = 0.3f;
    
    [Tooltip("Angle tolerance for good landings (degrees)")]
    public float goodLandingAngleTolerance = 15f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;
    private bool isGrounded;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showSpeedDisplay = true; // Show on-screen speed meter
    
    [Tooltip("Force reset to new default values on start (enable once, then disable)")]
    public bool forceResetToNewDefaults = false;
    
    // Track speed changes for volatility feedback
    private float lastFrameSpeed = 0f;
    private float speedChange = 0f;
    
    [Header("Distance Tracking")]
    [Tooltip("Total distance traveled (used for scoring and avalanche progression)")]
    public float distanceTravelled = 0f;
    
    [Tooltip("Distance scale - reduces how fast distance accumulates (0.2 = 5x slower progression)")]
    public float distanceScale = 0.2f; // Jerry moves fast, but distance counts slower!
    
    private float lastXPosition;
    
    [Header("Game State")]
    public bool isAlive = true;
    
    [Header("Release Timing (Crest Detection)")]
    [Tooltip("Frames after releasing tuck that allow launch conversion")]
    public int launchGracePeriodFrames = 10; // Increased for more forgiveness
    
    [Tooltip("Timing efficiency for perfect release (1.0 = perfect, 0.5 = OK, 0.2 = bad)")]
    public float timingEfficiencyPerfect = 1.0f;
    public float timingEfficiencyGood = 0.8f; // More forgiving
    public float timingEfficiencyBad = 0.5f; // More forgiving
    
    [Tooltip("Lookahead distance for crest detection (world units)")]
    public float crestLookaheadDistance = 8f; // Much longer warning!
    
    [Tooltip("Cooldown between launches (prevents spam)")]
    public float launchCooldown = 0.3f; // 300ms between launches
    private float timeSinceLastLaunch = 0f;
    
    [Header("Momentum Decay & Recovery")]
    [Tooltip("Speed bleed rate when over maxSpeed")]
    public float speedBleedRate = 0.95f;
    
    [Tooltip("Minimum speed to maintain momentum")]
    public float momentumThreshold = 8f;
    
    private bool wasGroundedLastFrame;
    private int framesSinceReleasedTuck = 999;
    private float tangentialSpeedAtRelease = 0f; // v_t when spacebar released
    private Vector2 lastVelocity;
    private float currentSlopeAngle = 0f;
    private float slopeAngleAhead = 0f;
    private bool isApproachingCrest = false;
    
    private bool isTucking;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Force reset option for reverting to defaults
        if (forceResetToNewDefaults)
        {
            groundStickForce = 120f;
            tuckAirDownForce = 180f;
            tuckGravityMultiplier = 1.8f;
            gravityAccelMultiplier = 80f;
            tuckAccelMultiplier = 2.0f;
            tuckDragCoefficient = 1.5f;
            normalDragCoefficient = 0.8f;
            uphillPenaltyMultiplier = 4.0f;
            speedToLaunchConversion = 0.4f;
            baseLaunchVelocity = 10f;
            releaseBoostMultiplier = 1.2f;
            maxLaunchVelocity = 50f;
            crestLookaheadDistance = 8f;
            launchGracePeriodFrames = 10;
            timingEfficiencyGood = 0.8f;
            timingEfficiencyBad = 0.5f;
            maxSpeed = 50f;
        }
        
        // Store original color for tuck visual feedback
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Enable interpolation for smooth movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // Gravity acts vertically only (no horizontal projection except through tuck)
        rb.gravityScale = 1.5f;
        
        // Disable rotation from physics (we control rotation manually)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Smooth collisions - prevents bouncing
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // NO linear drag - we control speed entirely through our physics
        rb.drag = 0f;
        
        // Start at comfortable speed
        currentSpeed = 15f;
        targetSpeed = 15f; // Initialize target
        speedVelocity = 0f;
        
        // Initialize distance tracking
        lastXPosition = transform.position.x;
        distanceTravelled = 0f;
        
        // Initialize launch cooldown (allow immediate first launch)
        timeSinceLastLaunch = 999f;
        
        // Player initialized
    }
    
    void Update()
    {
        // Don't allow input if caught by avalanche
        if (!isAlive) return;
        
        // Update launch cooldown timer
        timeSinceLastLaunch += Time.deltaTime;
        
        // Input - CHECK IF SPACEBAR IS WORKING
        bool wasTucking = isTucking;
        isTucking = Input.GetKey(KeyCode.Space);
        
        // Track when tuck is released
        if (!isTucking && wasTucking)
        {
            framesSinceReleasedTuck = 0;
            tangentialSpeedAtRelease = currentSpeed;
        }
        else if (!isTucking)
        {
            framesSinceReleasedTuck++;
        }
        else if (isTucking)
        {
            // Reset grace period while tucking
            framesSinceReleasedTuck = 999;
        }
        
        // Visual feedback - SPRITE SWAPPING + COLOR
        if (spriteRenderer != null)
        {
            // Swap sprite based on tuck state
            if (isTucking && tuckedSprite != null)
            {
                spriteRenderer.sprite = tuckedSprite;
            }
            else if (!isTucking && standingSprite != null)
            {
                spriteRenderer.sprite = standingSprite;
            }
            
            // Color feedback (tint the sprite) - NO YELLOW when tucking
            if (isTucking && isApproachingCrest)
            {
                spriteRenderer.color = Color.cyan; // CYAN = RELEASE NOW! (approaching crest while tucking)
            }
            else if (framesSinceReleasedTuck < launchGracePeriodFrames)
            {
                spriteRenderer.color = Color.green; // Green = Launch window!
            }
            else
            {
                spriteRenderer.color = Color.white; // White = normal
            }
        }
        
        
        // Check if grounded and detect crests
        CheckGround();
        DetectCrest();
        
        
        // CORE MECHANIC: Convert tangential speed to launch when leaving ground after release
        // ANTI-SPAM: Only allow launch if cooldown has passed
        if (wasGroundedLastFrame && !isGrounded && timeSinceLastLaunch >= launchCooldown)
        {
            // Player just left the ground - check if this was a timed release
            if (framesSinceReleasedTuck < launchGracePeriodFrames && !isTucking)
            {
                // Calculate timing efficiency based on crest proximity and release timing
                float timingEfficiency = CalculateReleaseEfficiency();
                
                // CONVERSION: tangentialSpeed → upward velocity with release boost
                // launchVelocity = currentVelocity * releaseBoostMultiplier
                float boostedSpeed = tangentialSpeedAtRelease * releaseBoostMultiplier;
                float speedLaunch = boostedSpeed * speedToLaunchConversion * timingEfficiency;
                float totalLaunch = baseLaunchVelocity + speedLaunch;
                totalLaunch = Mathf.Clamp(totalLaunch, baseLaunchVelocity, maxLaunchVelocity);
                
                // Apply launch velocity
                rb.velocity = new Vector2(rb.velocity.x, totalLaunch);
                
                // Reset cooldown
                timeSinceLastLaunch = 0f;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }
            // Still tucking = STAY GLUED (no launch, stick to terrain)
            else if (isTucking)
            {
                // Kill any upward velocity - player wants to stay stuck to ground
                if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y * 0.1f, 0f));
                }
            }
        }
        
        wasGroundedLastFrame = isGrounded;
    }
    
    void FixedUpdate()
    {
        // Core momentum physics
        HandleMovement();
        
        // Track distance traveled (only count forward movement)
        // Apply distance scale to slow down progression while keeping Jerry fast!
        float deltaX = transform.position.x - lastXPosition;
        if (deltaX > 0)
        {
            distanceTravelled += deltaX * distanceScale; // Scaled distance for better pacing
        }
        lastXPosition = transform.position.x;
    }
    
    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            Vector2.down, 
            groundCheckDistance, 
            groundLayer
        );
        
        isGrounded = hit.collider != null;
        
        if (isGrounded)
        {
            // Get slope angle
            Vector2 groundNormal = hit.normal;
            currentSlopeAngle = Vector2.SignedAngle(Vector2.up, groundNormal);
            
            // Compensate for hill rotation
            currentSlopeAngle += 15f;
            
            // Calculate speed change based on slope
            ApplySlopePhysics(currentSlopeAngle);
            
            // Prevent bouncing ONLY when not tucking (tucking should glue to terrain)
            if (!isTucking && currentSlopeAngle > 10f && rb.velocity.y > 0)
            {
                // Damp vertical velocity to prevent bouncing on uphills
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.7f);
            }
            
            // Smoothly rotate player to match ground angle
            float targetRotation = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                Quaternion.Euler(0, 0, targetRotation), 
                Time.deltaTime * 15f
            );
        }
        
        // Debug visualization
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, 
            isGrounded ? Color.green : Color.red);
    }
    
    void DetectCrest()
    {
        if (!isGrounded) 
        {
            isApproachingCrest = false;
            slopeAngleAhead = 0f;
            return;
        }
        
        // Raycast ahead to detect upcoming slope transition
        // Start HIGH ABOVE player to handle both uphill and downhill ahead
        Vector2 startPos = transform.position;
        Vector2 checkPos = startPos + Vector2.right * crestLookaheadDistance + Vector2.up * 50f; // Start 50 units ABOVE
        
        RaycastHit2D hitAhead = Physics2D.Raycast(
            checkPos,
            Vector2.down,
            200f, // VERY long raycast to handle any terrain
            groundLayer
        );
        
        if (hitAhead.collider != null)
        {
            Vector2 normalAhead = hitAhead.normal;
            slopeAngleAhead = Vector2.SignedAngle(Vector2.up, normalAhead) + 15f;
            
            // SIMPLIFIED CREST DETECTION:
            // If we're going downhill AND the slope ahead is less steep (flattening) = approaching crest
            bool isCurrentlyDownhill = currentSlopeAngle < -5f;
            bool isAheadFlatter = slopeAngleAhead > currentSlopeAngle + 5f; // Ahead is at least 5° flatter
            
            isApproachingCrest = isCurrentlyDownhill && isAheadFlatter && isTucking;
            
            // Debug visualization - draw the raycast
            Debug.DrawRay(checkPos, Vector2.down * 200f, isApproachingCrest ? Color.cyan : Color.yellow);
            
        }
        else
        {
            // Raycast didn't hit anything
            isApproachingCrest = false;
            slopeAngleAhead = 0f;
        }
    }
    
    void ApplySlopePhysics(float slopeAngle)
    {
        // SLOPE-BASED SPEED SYSTEM per your spec
        // acceleration = g * sin(θ) * multiplier
        // Dramatic fluctuation: 5-25 range with peaks to 40+
        
        bool isDownhill = slopeAngle < -3f;
        bool isUphill = slopeAngle > 3f;
        
        float slopeFactor = Mathf.Sin(slopeAngle * Mathf.Deg2Rad); // Positive uphill, negative downhill
        
        // GRAVITY-BASED ACCELERATION (core physics)
        float gravityAccel = gravityAccelMultiplier * slopeFactor; // g * sin(θ) * multiplier
        
        float dragCoefficient;
        float netAccel;
        
        if (isTucking && isDownhill)
        {
            // TUCKING DOWNHILL: Moderate acceleration with HIGH drag tradeoff
            netAccel = gravityAccel * tuckAccelMultiplier;
            dragCoefficient = tuckDragCoefficient;
        }
        else if (isTucking && isUphill)
        {
            // TUCKING UPHILL: SEVERE penalty
            netAccel = gravityAccel * uphillPenaltyMultiplier;
            dragCoefficient = tuckDragCoefficient * 1.5f;
        }
        else if (!isTucking && isDownhill)
        {
            // NOT TUCKING DOWNHILL: Natural gravity acceleration, moderate drag
            netAccel = gravityAccel;
            dragCoefficient = normalDragCoefficient;
        }
        else if (!isTucking && isUphill)
        {
            // NOT TUCKING UPHILL: Natural deceleration with increased drag
            netAccel = gravityAccel; // Normal decel
            dragCoefficient = normalDragCoefficient * 1.3f; // Increased drag
        }
        else
        {
            // FLAT TERRAIN: Passive slowdown (no free speed!)
            netAccel = 0f;
            dragCoefficient = normalDragCoefficient * 1.2f; // Slight drag even on flat
        }
        
        // Calculate TARGET speed (what we want to reach)
        targetSpeed += netAccel * Time.fixedDeltaTime;
        
        // Apply base drag to target
        float baseDrag = targetSpeed * dragCoefficient * Time.fixedDeltaTime;
        targetSpeed -= baseDrag;
        
        // Apply speed-proportional drag to target
        float dynamicDrag = targetSpeed * speedProportionalDrag * Time.fixedDeltaTime;
        targetSpeed -= dynamicDrag;
        
        // Dynamic clamping for target
        float effectiveCap = isGrounded ? baseSpeedCap : maxSpeed;
        targetSpeed = Mathf.Clamp(targetSpeed, minSpeed, effectiveCap);
        
        // SMOOTH transition from current to target speed (removes jitter!)
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);
        
        // Final safety clamp
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
    }
    
    void HandleMovement()
    {
        if (isGrounded)
        {
            // GROUND PHYSICS - Apply landing penalty if needed
            ApplyLandingPenalty();
            
            // Maintain horizontal momentum
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            
            // CRITICAL: If tucking on ground, GLUE Jerry to terrain
            if (isTucking)
            {
                // STRONG downward force - Jerry should follow every curve
                rb.AddForce(Vector2.down * groundStickForce, ForceMode2D.Force);
                
                // Kill any upward velocity when tucking (prevents any bounce/launch)
                if (rb.velocity.y > 0.1f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                }
            }
        }
        else
        {
            // AIR PHYSICS - Apply speed-dependent drag
            ApplyAirDrag();
            
            // Maintain horizontal momentum with air drag
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            
            // If tucking in air, pull DOWN to get back to ground quickly
            if (isTucking)
            {
                // Strong downward force - player wants to get back to ground
                rb.AddForce(Vector2.down * tuckAirDownForce, ForceMode2D.Force);
                
                // Increase gravity for faster descent
                rb.gravityScale = 1.5f * tuckGravityMultiplier;
            }
            else
            {
                // Normal gravity when not tucking
                rb.gravityScale = 1.5f;
            }
        }
        
        // Store velocity for landing penalty calculation
        lastVelocity = rb.velocity;
        
        // Clamp speed with momentum threshold
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
    }
    
    void ApplyAirDrag()
    {
        // AIR DRAG: Constant drag loss while airborne (per your spec)
        // Minimal drag - speed decreases slightly
        currentSpeed *= airDrag; // 0.995 = 0.5% loss per frame
        
        // If tucking in air, apply fast fall (slight increased descent)
        // This is handled in HandleMovement() via gravity multiplier
    }
    
    void ApplyLandingPenalty()
    {
        // Only apply penalty when first landing (not every frame on ground)
        if (!wasGroundedLastFrame && isGrounded)
        {
            // Calculate angle mismatch between velocity and ground normal
            Vector2 groundNormal = GetGroundNormal();
            if (groundNormal != Vector2.zero)
            {
                float velocityAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                float groundAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(velocityAngle, groundAngle));
                
                // Apply landing penalty based on angle mismatch
                if (angleDifference > goodLandingAngleTolerance)
                {
                    float penalty = (angleDifference - goodLandingAngleTolerance) / 90f * landingLossFactor;
                    currentSpeed *= (1f - penalty);
                }
            }
        }
    }
    
    Vector2 GetGroundNormal()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null ? hit.normal : Vector2.zero;
    }
    
    float CalculateReleaseEfficiency()
    {
        // Perfect timing = released at crest within grace window
        // Good timing = released near crest
        // Bad timing = released too early or not at crest
        
        // Frame-based component (closer to release = better)
        float frameEfficiency = 1f - (framesSinceReleasedTuck / (float)launchGracePeriodFrames);
        frameEfficiency = Mathf.Clamp01(frameEfficiency);
        
        // Crest-based component (was approaching crest when released?)
        float crestBonus = isApproachingCrest ? timingEfficiencyPerfect : timingEfficiencyGood;
        
        // Combine both factors
        float finalEfficiency = frameEfficiency * crestBonus;
        
        // Ensure minimum efficiency
        finalEfficiency = Mathf.Max(finalEfficiency, timingEfficiencyBad);
        
        return finalEfficiency;
    }
    
    void OnGUI()
    {
        if (!showSpeedDisplay) return;
        
        // Track speed change
        speedChange = currentSpeed - lastFrameSpeed;
        lastFrameSpeed = currentSpeed;
        
        // Display speed meter in top-left
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
        
        // Color code based on speed
        Color speedColor = Color.white;
        if (currentSpeed >= baseSpeedCap) speedColor = Color.yellow; // At cap
        if (currentSpeed >= maxSpeed * 0.8f) speedColor = Color.green; // High speed
        if (currentSpeed < 15f) speedColor = Color.red; // Low speed
        
        style.normal.textColor = speedColor;
        
        string speedText = $"SPEED: {currentSpeed:F1} / {baseSpeedCap}\n";
        speedText += $"Change: {(speedChange >= 0 ? "+" : "")}{speedChange:F2}/frame\n";
        speedText += $"State: {(isTucking ? "TUCK" : "FREE")} | {(isGrounded ? "GROUND" : "AIR")}\n";
        speedText += $"Slope: {currentSlopeAngle:F0}°";
        
        GUI.Label(new Rect(10, 10, 300, 100), speedText, style);
    }
    
    /// <summary>
    /// Called by AvalancheController when Jerry is caught.
    /// Freezes player control and physics.
    /// </summary>
    public void CaughtByAvalanche()
    {
        if (!isAlive) return; // Already dead
        
        isAlive = false;
        isTucking = false;
        
        // Freeze physics
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        
        // Visual feedback - turn red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }
}
