using UnityEngine;

/// <summary>
/// Makes an object follow the camera's position (useful for backgrounds)
/// </summary>
public class CameraFollower : MonoBehaviour
{
    [Header("Camera Reference")]
    [Tooltip("The camera to follow (will auto-detect Main Camera if not set)")]
    public Camera targetCamera;
    
    [Header("Follow Settings")]
    [Tooltip("Follow the camera's X position")]
    public bool followX = true;
    
    [Tooltip("Follow the camera's Y position")]
    public bool followY = true;
    
    [Tooltip("Follow the camera's Z position")]
    public bool followZ = false;
    
    [Tooltip("Offset from the camera position")]
    public Vector3 offset = Vector3.zero;
    
    private Vector3 startPosition;
    
    void Start()
    {
        // Auto-detect main camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        startPosition = transform.position;
        
        // Immediately snap to camera position on start (prevents initial offset)
        if (targetCamera != null)
        {
            Vector3 newPosition = transform.position;
            
            if (followX)
                newPosition.x = targetCamera.transform.position.x + offset.x;
            
            if (followY)
                newPosition.y = targetCamera.transform.position.y + offset.y;
            
            if (!followZ)
                newPosition.z = startPosition.z + offset.z;
            else
                newPosition.z = targetCamera.transform.position.z + offset.z;
            
            transform.position = newPosition;
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        Vector3 newPosition = transform.position;
        
        if (followX)
        {
            newPosition.x = targetCamera.transform.position.x + offset.x;
        }
        
        if (followY)
        {
            newPosition.y = targetCamera.transform.position.y + offset.y;
        }
        
        if (followZ)
        {
            newPosition.z = targetCamera.transform.position.z + offset.z;
        }
        else
        {
            // Keep the original Z if not following
            newPosition.z = startPosition.z + offset.z;
        }
        
        transform.position = newPosition;
    }
}

