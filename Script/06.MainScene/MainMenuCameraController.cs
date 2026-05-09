using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuCameraController : MonoBehaviour
{
    public Transform defaultRotation;  
    public Transform optionsRotation;  
    public float rotationSpeed = 2f; 

    public RawImage RawImage;
    public GameObject optionsPanel; 
    public GameObject mainPanel; 

    private ButtonScaleEffect[] buttonEffects;
    private ButtonScaleEffect[] optionsMenuButtons;

    private bool isRotating = false;  

    void Start()
    {
        transform.rotation = defaultRotation.rotation; 
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
        RawImage.enabled = true;

        buttonEffects = FindObjectsByType<ButtonScaleEffect>(FindObjectsSortMode.None);
        optionsMenuButtons = optionsPanel.GetComponentsInChildren<ButtonScaleEffect>(true);
    }

    public void LookAtOptions()
    {
        if (!isRotating)
        {
            Debug.Log("🔹 카메라가 옵션 메뉴를 바라봅니다.");
            ResetAllButtons();
            StartCoroutine(RotateCamera(optionsRotation, true));
        }
    }

    public void LookAtMain()
    {
        if (!isRotating)
        {
            Debug.Log("🔹 카메라가 원래 방향으로 돌아갑니다.");
            ResetAllButtons();
            StartCoroutine(RotateCamera(defaultRotation, false));
        }
    }

    private IEnumerator RotateCamera(Transform targetRotation, bool openOptions)
    {
        isRotating = true;
        float elapsedTime = 0f;
        Quaternion startRot = transform.rotation;

        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);

        RawImage.enabled = false;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(startRot, targetRotation.rotation, elapsedTime);
            yield return null;
        }

        transform.rotation = targetRotation.rotation; 

        mainPanel.SetActive(!openOptions);
        optionsPanel.SetActive(openOptions);

        if (!openOptions)
        {
            RawImage.enabled = true;
        }
        else
        {
            RawImage.enabled = false;
        }

        isRotating = false;
    }
    private void ResetAllButtons()
    {
        foreach (var buttonEffect in buttonEffects)
        {
            buttonEffect.ResetScale();
        }
        foreach (var buttonEffect in optionsMenuButtons)
        {
            buttonEffect.ResetScale();
        }
    }
}
