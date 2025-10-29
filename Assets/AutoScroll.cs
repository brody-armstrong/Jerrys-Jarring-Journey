using UnityEngine;

/// <summary>
/// Automatically scrolls a sprite horizontally at a constant speed with seamless looping.
/// Creates a duplicate sprite that follows behind for infinite scrolling.
/// </summary>
public class AutoScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Speed of horizontal scrolling (negative = left, positive = right)")]
    public float scrollSpeed = -0.5f;
    
    [Header("Seamless Looping")]
    [Tooltip("Width of the sprite (auto-detected if 0)")]
    public float spriteWidth = 0f;
    
    private GameObject cloneSprite;
    private SpriteRenderer spriteRenderer;
    private float actualWidth;
    
    void Start()
    {
        // Get sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Auto-detect sprite width if not manually set
            if (spriteWidth == 0f)
            {
                actualWidth = spriteRenderer.bounds.size.x;
            }
            else
            {
                actualWidth = spriteWidth;
            }
            
            // Create a duplicate sprite positioned next to this one
            cloneSprite = new GameObject(gameObject.name + "_Clone");
            SpriteRenderer cloneSR = cloneSprite.AddComponent<SpriteRenderer>();
            cloneSR.sprite = spriteRenderer.sprite;
            cloneSR.sortingLayerName = spriteRenderer.sortingLayerName;
            cloneSR.sortingOrder = spriteRenderer.sortingOrder;
            
            // Position clone to the right (for left scrolling) or left (for right scrolling)
            cloneSprite.transform.position = transform.position + Vector3.right * actualWidth;
            cloneSprite.transform.localScale = transform.localScale;
            cloneSprite.transform.parent = transform.parent; // Same parent as original
        }
    }
    
    void Update()
    {
        // Scroll both original and clone
        transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
        
        if (cloneSprite != null)
        {
            cloneSprite.transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
        }
        
        // When original scrolls off screen, reset both positions
        if (scrollSpeed < 0) // Scrolling left
        {
            if (transform.position.x <= -actualWidth)
            {
                transform.position += Vector3.right * actualWidth * 2;
            }
            if (cloneSprite != null && cloneSprite.transform.position.x <= -actualWidth)
            {
                cloneSprite.transform.position += Vector3.right * actualWidth * 2;
            }
        }
        else if (scrollSpeed > 0) // Scrolling right
        {
            if (transform.position.x >= actualWidth)
            {
                transform.position -= Vector3.right * actualWidth * 2;
            }
            if (cloneSprite != null && cloneSprite.transform.position.x >= actualWidth)
            {
                cloneSprite.transform.position -= Vector3.right * actualWidth * 2;
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up clone when this object is destroyed
        if (cloneSprite != null)
        {
            Destroy(cloneSprite);
        }
    }
}

