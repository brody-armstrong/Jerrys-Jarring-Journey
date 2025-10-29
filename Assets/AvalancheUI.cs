using UnityEngine;
using TMPro;

/// <summary>
/// Displays "AVALANCHE WARNING" when avalanche is entering camera view or visible
/// </summary>
public class AvalancheUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Avalanche controller")]
    public AvalancheController avalanche;
    
    [Tooltip("Warning text (TextMeshPro)")]
    public TextMeshProUGUI warningText;
    
    [Header("Warning Settings")]
    [Tooltip("How far off-screen (in world units) to start showing warning")]
    public float offScreenWarningBuffer = 5f; // Show warning 5 units before it enters screen
    
    [Tooltip("Flash speed")]
    public float flashSpeed = 3f;
    
    private Camera mainCamera;
    private bool isWarningActive = false;
    private AudioManager audioManager;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
        
        if (avalanche == null)
        {
            avalanche = FindObjectOfType<AvalancheController>();
        }
        
        audioManager = FindObjectOfType<AudioManager>();
    }
    
    void Update()
    {
        if (avalanche == null || warningText == null || mainCamera == null) return;
        
        // Get avalanche position in viewport space (0-1 range)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(avalanche.transform.position);
        
        // Calculate left edge of screen in world space
        Vector3 leftEdgeViewport = new Vector3(0, 0.5f, mainCamera.nearClipPlane);
        Vector3 leftEdgeWorld = mainCamera.ViewportToWorldPoint(leftEdgeViewport);
        
        // Distance from avalanche to left edge of screen
        float distanceToScreen = leftEdgeWorld.x - avalanche.transform.position.x;
        
        // Show warning if:
        // 1. Avalanche is ABOUT to enter screen (within buffer distance to the left)
        // 2. OR avalanche is VISIBLE on screen (viewport X between 0 and 1)
        bool isNearScreen = distanceToScreen > 0 && distanceToScreen < offScreenWarningBuffer;
        bool isOnScreen = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
        
        bool shouldShowWarning = isNearScreen || isOnScreen;
        
        if (shouldShowWarning && !isWarningActive)
        {
            // Show warning
            warningText.gameObject.SetActive(true);
            isWarningActive = true;
            
            // Start avalanche rumble sound
            if (audioManager != null)
            {
                audioManager.StartAvalancheRumble();
            }
        }
        else if (!shouldShowWarning && isWarningActive)
        {
            // Hide warning
            warningText.gameObject.SetActive(false);
            isWarningActive = false;
            
            // Stop avalanche rumble sound
            if (audioManager != null)
            {
                audioManager.StopAvalancheRumble();
            }
        }
        
        // Flash effect when active
        if (isWarningActive)
        {
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
            Color color = warningText.color;
            color.a = alpha;
            warningText.color = color;
        }
    }
}
