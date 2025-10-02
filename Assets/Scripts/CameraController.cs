using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CarController theCarController;
    public CarControllerV2 theCarControllerV2;
    public Vector3 offsetDirection;
    public float minDistance = 20f;
    public float maxDistance = 50f;
    public float activeDistance = 10f;

    void Start()
    {
        theCarController = FindFirstObjectByType<CarController>();

        if (theCarController == null)
        {
            Debug.LogError("No CarController found in the scene.");
        }

        if (theCarController != null)
        {
            offsetDirection = transform.position - theCarController.transform.position;
        }

        theCarControllerV2 = FindFirstObjectByType<CarControllerV2>();

        if (theCarControllerV2 == null)
        {
            Debug.LogError("No CarControllerV2 found in the scene.");
        }

        if (theCarControllerV2 != null)
        {
            offsetDirection = transform.position - theCarControllerV2.transform.position;
        }

        activeDistance = minDistance;

        offsetDirection.Normalize();
    }

    void Update()
    {
        if (theCarController != null)
        {
            activeDistance = minDistance + (maxDistance - minDistance) * (theCarController.theRB.linearVelocity.magnitude / theCarController.maxSpeed);
            transform.position = theCarController.transform.position + (offsetDirection * activeDistance);
        }

        if (theCarControllerV2 != null) 
        {
            activeDistance = minDistance + (maxDistance - minDistance) * (theCarControllerV2.rb.linearVelocity.magnitude / theCarControllerV2.maxSpeed);
            transform.position = theCarControllerV2.transform.position + (offsetDirection * activeDistance);
        }
    }
}
