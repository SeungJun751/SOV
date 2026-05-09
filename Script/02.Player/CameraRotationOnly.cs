using UnityEngine;

public class CameraRotationOnly : MonoBehaviour
{
    public Transform cameraTransform;
    public float rotationSpeed = 2.0f;
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -5); 

    private bool isThirdPerson = false;
    private float xRotation = 0f;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        LockCursor();
    }

    void Update()
    {
        HandleCameraSwitch();
        RotateCamera();
        UpdateCameraPosition();
    }

    private void HandleCameraSwitch()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isThirdPerson = !isThirdPerson;
        }
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        transform.Rotate(Vector3.up * mouseX);

        if (!isThirdPerson) 
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void UpdateCameraPosition()
    {
        if (isThirdPerson)
        {
            Vector3 targetPosition = transform.position + transform.TransformDirection(thirdPersonOffset);
            cameraTransform.position = targetPosition;
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f); 
        }
        else
        {
            Vector3 forwardOffset = transform.forward * 0.6f; 
            cameraTransform.position = transform.position + forwardOffset;
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
