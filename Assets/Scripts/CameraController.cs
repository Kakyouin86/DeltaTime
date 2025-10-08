using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CarController theCarController;
    public CarControllerV2 theCarControllerV2;
    public Vector3 offsetDirectionCont1;
    public Vector3 offsetDirectionCont2;
    public float minDistance = 20f;
    public float maxDistance = 50f;
    public float activeDistance = 10f;
    public Transform startTargetOffset;

    void Start()
    {
        theCarController = FindFirstObjectByType<CarController>();

        if (theCarController == null)
        {
            //Debug.LogError("No CarController found in the scene.");
        }

        if (theCarController != null)
        {
            offsetDirectionCont1 = transform.position - startTargetOffset.transform.position;
        }

        theCarControllerV2 = FindFirstObjectByType<CarControllerV2>();

        if (theCarControllerV2 == null)
        {
            //Debug.LogError("No CarControllerV2 found in the scene.");
        }

        if (theCarControllerV2 != null)
        {
            offsetDirectionCont2 = transform.position - startTargetOffset.transform.position;
        }

        activeDistance = minDistance;

        offsetDirectionCont1.Normalize();
        offsetDirectionCont2.Normalize();
    }

    void Update()
    {
        if (theCarController != null)
        {
            activeDistance = minDistance + (maxDistance - minDistance) * (theCarController.theRB.linearVelocity.magnitude / theCarController.maxSpeed);
            transform.position = theCarController.transform.position + (offsetDirectionCont1 * activeDistance);
        }

        if (theCarControllerV2 != null)
        {
            activeDistance = minDistance + (maxDistance - minDistance) * (theCarControllerV2.theRB.linearVelocity.magnitude / theCarControllerV2.maxSpeed);
            transform.position = theCarControllerV2.transform.position + (offsetDirectionCont2 * activeDistance);
        }
    }
}