using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Range(0f, 200f)]
    public float mouseSensitivity = 100f; 
    
    public Transform playerBody; 
    private float xRotation = 0f; 
    private bool isRotationEnabled = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);
        Debug.Log("불러온 마우스 감도: " + mouseSensitivity);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (isRotationEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; 
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; 

            xRotation -= mouseY; 
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); /

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
            playerBody.Rotate(Vector3.up * mouseX); 
        }
    }
    public void SetRotationEnabled(bool enabled)
    {
        isRotationEnabled = enabled;
        if (!enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void UpdateSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }
}
