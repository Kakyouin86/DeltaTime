using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float rotationSpeed = 10.0f;

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 rotation = new Vector3(-verticalInput, horizontalInput, 0) * rotationSpeed * Time.deltaTime;

        transform.eulerAngles += rotation;
    } 
}
