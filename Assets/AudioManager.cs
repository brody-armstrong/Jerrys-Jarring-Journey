using UnityEngine;

/// <summary>
/// Manages all game audio - simple and easy!
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Audio source for looping sounds (avalanche rumble)")]
    public AudioSource loopingSource;
    
    [Tooltip("Audio source for one-shot sounds (death, button clicks)")]
    public AudioSource oneShotSource;
    
    [Header("Sound Clips")]
    [Tooltip("Avalanche rumble sound (looping)")]
    public AudioClip avalancheRumble;
    
    [Tooltip("Death/crash sound")]
    public AudioClip deathSound;
    
    [Tooltip("Button click sound")]
    public AudioClip buttonClick;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float avalancheVolume = 0.5f;
    
    [Range(0f, 1f)]
    public float deathVolume = 1f;
    
    [Range(0f, 1f)]
    public float buttonVolume = 0.7f;
    
    private bool isAvalancheRumbling = false;
    
    void Update()
    {
        // Update avalanche rumble volume based on distance
        if (isAvalancheRumbling && loopingSource != null)
        {
            loopingSource.volume = avalancheVolume;
        }
    }
    
    /// <summary>
    /// Start playing avalanche rumble sound
    /// </summary>
    public void StartAvalancheRumble()
    {
        if (loopingSource == null || avalancheRumble == null) return;
        
        if (!isAvalancheRumbling)
        {
            loopingSource.clip = avalancheRumble;
            loopingSource.loop = true;
            loopingSource.volume = avalancheVolume;
            loopingSource.Play();
            isAvalancheRumbling = true;
            
            Debug.Log("🔊 Avalanche rumble started");
        }
    }
    
    /// <summary>
    /// Stop avalanche rumble sound
    /// </summary>
    public void StopAvalancheRumble()
    {
        if (loopingSource == null) return;
        
        if (isAvalancheRumbling)
        {
            loopingSource.Stop();
            isAvalancheRumbling = false;
            
            Debug.Log("🔇 Avalanche rumble stopped");
        }
    }
    
    /// <summary>
    /// Play death sound
    /// </summary>
    public void PlayDeathSound()
    {
        if (oneShotSource == null || deathSound == null) return;
        
        oneShotSource.PlayOneShot(deathSound, deathVolume);
        Debug.Log("💀 Death sound played");
    }
    
    /// <summary>
    /// Play button click sound
    /// </summary>
    public void PlayButtonClick()
    {
        if (oneShotSource == null || buttonClick == null) return;
        
        oneShotSource.PlayOneShot(buttonClick, buttonVolume);
        Debug.Log("🔊 Button click played");
    }
}

