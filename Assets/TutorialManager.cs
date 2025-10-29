using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro support

/// <summary>
/// Manages tutorial prompts that appear during gameplay to teach mechanics.
/// Shows prompts once, then disappears forever (per session).
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshPro component for tutorial prompts")]
    public TextMeshProUGUI tutorialText;
    
    [Header("Tutorial Timing")]
    [Tooltip("Delay before showing first tutorial prompt")]
    public float initialDelay = 2f;
    
    [Tooltip("How long each prompt stays on screen")]
    public float promptDuration = 3f;
    
    [Tooltip("Fade in/out time")]
    public float fadeDuration = 0.5f;
    
    [Header("Player Reference")]
    public PlayerController player;
    public AvalancheController avalanche;
    
    [Header("Tutorial Steps")]
    private bool tutorialComplete = false;
    private bool shownTuckPrompt = false;
    private bool shownReleasePrompt = false;
    private bool shownAvalanchePrompt = false;
    
    void Start()
    {
        // Hide text initially
        if (tutorialText != null)
        {
            tutorialText.enabled = false;
        }
        
        // Start tutorial sequence
        StartCoroutine(TutorialSequence());
    }
    
    void Update()
    {
        // Tutorial sequence handles everything - no dynamic avalanche check needed
    }
    
    IEnumerator TutorialSequence()
    {
        // Wait initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Step 1: Teach Tuck
        if (!shownTuckPrompt)
        {
            yield return ShowPrompt("Hold SPACE to Tuck and Build Speed!", promptDuration);
            shownTuckPrompt = true;
            yield return new WaitForSeconds(1f);
        }
        
        // Step 2: Teach Release
        if (!shownReleasePrompt)
        {
            yield return ShowPrompt("Release Before Hill Crests to Jump!", promptDuration);
            shownReleasePrompt = true;
            yield return new WaitForSeconds(1f);
        }
        
        // Step 3: Avalanche Warning
        if (!shownAvalanchePrompt)
        {
            yield return ShowPrompt("Stay Ahead of the Avalanche!", promptDuration);
            shownAvalanchePrompt = true;
        }
        
        // Tutorial complete!
        tutorialComplete = true;
    }
    
    IEnumerator ShowPrompt(string message, float duration)
    {
        if (tutorialText == null) yield break;
        
        // Set text
        tutorialText.text = message;
        tutorialText.enabled = true;
        
        // Fade in
        yield return FadeText(0f, 1f, fadeDuration);
        
        // Stay visible
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return FadeText(1f, 0f, fadeDuration);
        
        // Hide
        tutorialText.enabled = false;
    }
    
    IEnumerator FadeText(float from, float to, float duration)
    {
        if (tutorialText == null) yield break;
        
        float elapsed = 0f;
        Color color = tutorialText.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            tutorialText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        tutorialText.color = new Color(color.r, color.g, color.b, to);
    }
}

