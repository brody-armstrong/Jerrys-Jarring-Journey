using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickCameraFollow : MonoBehaviour
{
    public Transform player;
    
    [Header("Camera Positioning")]
    [Tooltip("Horizontal offset (negative = Jerry more to the right, see more ahead)")]
    public float xOffset = -1f;
    
    [Tooltip("Vertical offset (0 = centered, positive = Jerry lower, see more above)")]
    public float yOffset = 1f;
    
    [Tooltip("Camera orthographic size (higher = zoom out, see more terrain)")]
    public float cameraSize = 6f;
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = cameraSize;
        }
    }
    
    void LateUpdate()
    {
        if (player != null)
        {
            // Direct follow - no lerp to avoid jitter
            // Jerry positioned more centered, with view showing terrain above (for avalanche)
            Vector3 targetPosition = player.position + new Vector3(xOffset, yOffset, -10f);
            targetPosition.z = -10f; // Keep camera at fixed Z
            
            transform.position = targetPosition;
        }
    }
}
