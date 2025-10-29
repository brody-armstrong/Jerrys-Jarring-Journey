using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    
    [Header("Parallax Settings")]
    public float parallaxSpeed = 0.5f; // 0 = static, 1 = moves with camera
    public bool scrollVertical = true;
    public bool scrollHorizontal = true;
    
    private Vector3 lastCameraPosition;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        lastCameraPosition = cameraTransform.position;
    }
    
    void LateUpdate()
    {
        // Calculate camera movement
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // Apply parallax effect
        Vector3 newPosition = transform.position;
        
        if (scrollHorizontal)
        {
            newPosition.x += deltaMovement.x * parallaxSpeed;
        }
        
        if (scrollVertical)
        {
            newPosition.y += deltaMovement.y * parallaxSpeed;
        }
        
        transform.position = newPosition;
        lastCameraPosition = cameraTransform.position;
    }
}

