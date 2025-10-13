// HealthBar.cs

using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        /*healthBarFill.transform.localScale = new Vector3(
            currentHealth / maxHealth,
            healthBarFill.transform.localScale.y,
            healthBarFill.transform.localScale.z
            ); */
        
        healthBarFill.fillAmount = currentHealth / maxHealth; //No need to worry about pivots
    }

    // Makes the health bar always face the camera
    private void LateUpdate()
    {
        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
            cameraTransform.rotation * Vector3.up);
    }
}