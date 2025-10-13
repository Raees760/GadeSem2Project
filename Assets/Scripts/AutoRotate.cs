using UnityEngine;

/// <summary>
/// Continuously rotates the GameObject this script is attached to
/// around its vertical (Y) axis.
/// </summary>
public class AutoRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}