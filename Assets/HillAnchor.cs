using UnityEngine;

/// <summary>
/// Tracks the connection points of a hill segment for seamless chaining.
/// Attach this to each hill prefab.
/// </summary>
public class HillAnchor : MonoBehaviour
{
    [Header("Connection Points")]
    [Tooltip("The point where this hill starts (usually at the prefab's origin)")]
    public Transform startAnchor;
    
    [Tooltip("The point where this hill ends (where the next hill should connect)")]
    public Transform endAnchor;
    
    [Header("Debug Visualization")]
    public bool showGizmos = true;
    public float gizmoSize = 0.5f;
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw start anchor in green
        if (startAnchor != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startAnchor.position, gizmoSize);
            Gizmos.DrawLine(startAnchor.position, startAnchor.position + Vector3.up * 2f);
        }
        
        // Draw end anchor in red
        if (endAnchor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endAnchor.position, gizmoSize);
            Gizmos.DrawLine(endAnchor.position, endAnchor.position + Vector3.up * 2f);
        }
        
        // Draw connection line
        if (startAnchor != null && endAnchor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startAnchor.position, endAnchor.position);
        }
    }
}

