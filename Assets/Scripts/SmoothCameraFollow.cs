using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target to follow (car)")]
    public Transform target;

    [Header("Offset from the target")]
    public Vector3 offset = new Vector3(0, 5, -10);

    [Header("Smoothness factor (0 = no follow, 1 = instant follow)")]
    [Range(0f, 1f)] public float smoothSpeed = 0.125f;

    [Header("Look at the target?")]
    public bool lookAtTarget = true;

    void LateUpdate()
    {
        if (!target) return;

        // Desired camera position
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Smooth movement
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Camera orientation
        if (lookAtTarget)
        {
            // Look at the target but keep camera upright (no rolling with car)
            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0; // Ignore vertical rotation from the car
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed);
            }
        }
        else
        {
            // If not looking at target, just keep world upright
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), smoothSpeed);
        }
    }
}
