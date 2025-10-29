using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JERRY'S JARRING JOURNEY - Hill Generator
/// 
/// Procedurally generates infinite terrain using hill prefabs.
/// Spawns hills ahead of player and despawns old hills for performance.
/// Uses 4 varied hill prefabs that chain seamlessly for visual variety.
/// </summary>
public class HillGenerator : MonoBehaviour
{
    public enum ConnectionMode
    {
        AlignAnchors,           // Connect anchors exactly (may create lips if scales differ)
        MatchGroundHeight,      // Keep ground at consistent height (prevents lips)
        MatchStartAnchorHeight  // Always connect at the same Y as start anchors (ignores end anchor Y)
    }
    [Header("Hill Prefab Setup")]
    [Tooltip("The hill prefabs to randomly choose from (all must have HillAnchor component)")]
    public GameObject[] hillPrefabs;
    
    [Header("Initial Generation")]
    [Tooltip("How many hills to spawn at start (lower = better performance)")]
    public int initialHillCount = 5;
    
    [Tooltip("Starting Y position for hill generation (use negative values to place hills lower on screen)")]
    public float startingYPosition = -3f; // Lowered to fill bottom gaps
    
    [Header("Player Tracking")]
    [Tooltip("Reference to the player transform for endless generation")]
    public Transform player;
    
    [Tooltip("Spawn new hills when player is this close to the last hill")]
    public float spawnAheadDistance = 25f;
    
    [Tooltip("Remove hills this far behind the player")]
    public float cleanupDistance = 30f;
    
    [Header("Connection Settings")]
    [Tooltip("How to handle height differences between hill variants")]
    public ConnectionMode connectionMode = ConnectionMode.AlignAnchors;
    
    [Tooltip("When using MatchGroundHeight mode, offset from anchor Y position to use as ground reference")]
    public float groundHeightOffset = 0f;
    
    [Header("Downhill Slope")]
    [Tooltip("Rotate hills to create a downhill slope (negative = downward, in degrees)")]
    public float hillRotation = -15f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    
    // Internal tracking
    private List<GameObject> activeHills = new List<GameObject>();
    private GameObject lastHill;
    private Vector3 nextSpawnPosition;
    
    void Start()
    {
        // Initialize spawn position - start where the player is (or at origin if no player)
        float startX = (player != null) ? player.position.x : 0f;
        nextSpawnPosition = new Vector3(startX, startingYPosition, 0f);
        
        // Validate setup
        if (hillPrefabs == null || hillPrefabs.Length == 0)
        {
            Debug.LogError("HillGenerator: No hill prefabs assigned!");
            return;
        }
        
        // Validate all prefabs have HillAnchor components
        foreach (GameObject prefab in hillPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("HillGenerator: One of the hill prefabs is null!");
                return;
            }
            
            HillAnchor anchor = prefab.GetComponent<HillAnchor>();
            if (anchor == null)
            {
                Debug.LogError($"HillGenerator: Hill prefab '{prefab.name}' must have HillAnchor component!");
                return;
            }
        }
        
        // Generate initial hills
        for (int i = 0; i < initialHillCount; i++)
        {
            SpawnNextHill();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"HillGenerator: Spawned {initialHillCount} initial hills at Y = {startingYPosition}");
        }
    }
    
    void Update()
    {
        if (player == null || lastHill == null) return;
        
        // Check if we need to spawn more hills ahead
        HillAnchor lastAnchor = lastHill.GetComponent<HillAnchor>();
        if (lastAnchor != null && lastAnchor.endAnchor != null)
        {
            float distanceToEnd = lastAnchor.endAnchor.position.x - player.position.x;
            
            if (distanceToEnd < spawnAheadDistance)
            {
                SpawnNextHill();
            }
        }
        
        // Cleanup old hills behind player
        CleanupOldHills();
    }
    
    void SpawnNextHill()
    {
        if (hillPrefabs == null || hillPrefabs.Length == 0) return;
        
        // Randomly select a hill prefab
        GameObject selectedPrefab = hillPrefabs[Random.Range(0, hillPrefabs.Length)];
        
        // Instantiate the hill at the next spawn position with rotation
        Quaternion rotation = Quaternion.Euler(0, 0, hillRotation);
        GameObject newHill = Instantiate(selectedPrefab, nextSpawnPosition, rotation, transform);
        newHill.name = $"Hill_{activeHills.Count}_{selectedPrefab.name}";
        
        HillAnchor anchor = newHill.GetComponent<HillAnchor>();
        if (anchor == null)
        {
            Debug.LogError("HillGenerator: Hill prefab missing HillAnchor component!");
            Destroy(newHill);
            return;
        }
        
        // Connect this hill to the previous one
        if (lastHill != null)
        {
            HillAnchor lastAnchor = lastHill.GetComponent<HillAnchor>();
            if (lastAnchor != null && lastAnchor.endAnchor != null && anchor.startAnchor != null)
            {
                Vector3 offset = Vector3.zero;
                
                if (connectionMode == ConnectionMode.AlignAnchors)
                {
                    // Connect anchors exactly (may create height differences)
                    Vector3 connectionPoint = lastAnchor.endAnchor.position;
                    Vector3 currentStartPoint = anchor.startAnchor.position;
                    offset = connectionPoint - currentStartPoint;
                }
                else if (connectionMode == ConnectionMode.MatchGroundHeight)
                {
                    // Match X position and ground height to prevent lips
                    Vector3 lastEndPos = lastAnchor.endAnchor.position;
                    Vector3 currentStartPos = anchor.startAnchor.position;
                    
                    // Align X to connect horizontally
                    offset.x = lastEndPos.x - currentStartPos.x;
                    
                    // Keep ground at consistent height (using offset if needed)
                    float targetGroundHeight = lastEndPos.y + groundHeightOffset;
                    float currentGroundHeight = currentStartPos.y + groundHeightOffset;
                    offset.y = targetGroundHeight - currentGroundHeight;
                    
                    offset.z = 0; // 2D game
                }
                else if (connectionMode == ConnectionMode.MatchStartAnchorHeight)
                {
                    // Connect horizontally, and keep all hills at same start anchor Y height
                    // This ignores the end anchor Y position to prevent lips
                    Vector3 lastEndPos = lastAnchor.endAnchor.position;
                    Vector3 lastStartPos = lastAnchor.startAnchor.position;
                    Vector3 currentStartPos = anchor.startAnchor.position;
                    
                    // Connect at the end's X position
                    offset.x = lastEndPos.x - currentStartPos.x;
                    
                    // But keep the Y at the same level as the previous hill's START anchor
                    // (ignoring the height difference between start and end anchors)
                    offset.y = lastStartPos.y - currentStartPos.y;
                    
                    offset.z = 0; // 2D game
                }
                
                // Adjust the entire hill
                newHill.transform.position += offset;
                
                if (showDebugLogs && offset.magnitude > 0.01f)
                {
                    Debug.Log($"HillGenerator: Applied {connectionMode} offset {offset} to {newHill.name}");
                }
            }
        }
        
        // Update next spawn position to the end of this hill
        if (anchor.endAnchor != null)
        {
            nextSpawnPosition = anchor.endAnchor.position;
        }
        else
        {
            Debug.LogWarning($"HillGenerator: {newHill.name} has no end anchor! Using default offset.");
            nextSpawnPosition += new Vector3(10f, 0f, 0f); // Fallback
        }
        
        // Track the hill
        activeHills.Add(newHill);
        lastHill = newHill;
    }
    
    void CleanupOldHills()
    {
        if (player == null) return;
        
        // Remove hills that are far behind the player
        for (int i = activeHills.Count - 1; i >= 0; i--)
        {
            if (activeHills[i] == null) continue;
            
            float distanceBehind = player.position.x - activeHills[i].transform.position.x;
            
            if (distanceBehind > cleanupDistance)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"HillGenerator: Removing {activeHills[i].name} (behind player by {distanceBehind:F1} units)");
                }
                
                Destroy(activeHills[i]);
                activeHills.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Clears all hills and regenerates from scratch.
    /// Useful for testing or resetting the terrain.
    /// </summary>
    public void RegenerateAll()
    {
        // Clear existing hills
        foreach (GameObject hill in activeHills)
        {
            if (hill != null) Destroy(hill);
        }
        activeHills.Clear();
        
        // Reset spawn position
        nextSpawnPosition = new Vector3(0f, startingYPosition, 0f);
        lastHill = null;
        
        // Regenerate
        for (int i = 0; i < initialHillCount; i++)
        {
            SpawnNextHill();
        }
        
        Debug.Log("HillGenerator: Regenerated all hills");
    }
}

