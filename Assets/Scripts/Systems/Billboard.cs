using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Rotate the object to face the camera but only around the Y axis
            Vector3 targetPosition = mainCamera.transform.position;
            targetPosition.y = transform.position.y; // Lock Y to current height
            
            transform.LookAt(targetPosition);
            
            // Flip 180 if needed (often required for sprites)
            // transform.Rotate(0, 180, 0); 
            // In the previous code we looked 'away' or added 180 contextually. 
            // Standard LookAt faces Forward (Z+). Sprites often face Z-.
            // Let's try standard LookAt + 180 rotation fix or just look away.
            // transform.LookAt(transform.position - (mainCamera.transform.position - transform.position));
             
             // Let's stick to the previous logic but constraint axis.
             // Previous logic: transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
             // This logic matches camera rotation exactly, causing the tilt.
             
             // NEW LOGIC: Look at camera, flat.
             Vector3 direction = mainCamera.transform.position - transform.position;
             direction.y = 0; // Flatten
             
             if (direction != Vector3.zero)
             {
                 Quaternion lookRotation = Quaternion.LookRotation(-direction); // Negative direction often needed for sprites to face camera
                 transform.rotation = lookRotation;
             }
        }
    }
}
