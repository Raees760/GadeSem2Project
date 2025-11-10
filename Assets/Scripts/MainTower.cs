using UnityEngine;
using UnityEngine.Rendering; // Needed to access volume and post-processing effects
using UnityEngine.Rendering.HighDefinition; //for HDRP effects like Vignette

public class MainTower : ProjectileShooterTower // Inherits from Projectile tower
{
    [Header("VFX")]
    private Volume postProcessVolume; 
    private Vignette vignetteEffect; // The vignette component in the ppv
    
    protected override void Start()
    {
        base.Start();
        
        // Post Processing
        // Plug and play - automatically find the active Volume component in the entire scene.
        postProcessVolume = FindObjectOfType<Volume>();

        if (postProcessVolume != null) // If a Volume was found
        {
            // try to get the Vignette component from its profile.
            if (postProcessVolume.profile.TryGet(out vignetteEffect))
            {
                vignetteEffect.intensity.value = 0f; 
            }
            else
            {
                Debug.LogWarning("Main Tower found a Post Process Volume, but it's missing a Vignette override. The low-health effect will be disabled.");
                vignetteEffect = null; 
            }
        }
        else
        {
            Debug.LogWarning("Main Tower could not find any active Post Process Volume in the scene. The low-health effect will be disabled.");
        }
    }
    protected override void Update()
    {
        base.Update(); // Run the original tower logic

        if (vignetteEffect != null)
        {
            float healthPercent = Health / MaxHealth;
            float targetIntensity = 0f;

            // Only show the effect below 35% health
            if (healthPercent < 0.35f)
            {
                // The lower the health, the stronger the base intensity (from 0 to 1).
                // We use 1 minus the ratio to invert it.
                float baseIntensity = 1f - (healthPercent / 0.35f);
                
                // Add a pulsating effect using a sine wave
                float pulse = 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f; // Pulsates between 0.5 and 1.0

                // The final target intensity is a combination of both.
                targetIntensity = baseIntensity * pulse;
            }

            // Smoothly change the intensity to avoid sudden jumps.
            // Note: We access the value with '.value'
            vignetteEffect.intensity.value = Mathf.Lerp(vignetteEffect.intensity.value, targetIntensity, Time.deltaTime * 3f);
        }
    }
    protected override void Die()
    {
        if (vignetteEffect != null) vignetteEffect.intensity.value = 0f;
        
        GameManager.Instance.GameOver();
        base.Die();
    }
    
}