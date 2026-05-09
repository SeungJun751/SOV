using UnityEngine;

public class UIPanelController : MonoBehaviour
{
    public GameObject targetUIPanel;

    public GameObject openButton;

    void Awake()
    {
        if (targetUIPanel != null)
        {
            targetUIPanel.SetActive(false);
        }
        if (openButton != null)
        {
            openButton.SetActive(true);
        }
    }

    public void OpenPanel()
    {
        if (targetUIPanel != null)
        {
            targetUIPanel.SetActive(true);
        }
        if (openButton != null)
        {
            openButton.SetActive(false); 
        }
    }

    public void ClosePanel()
    {
        if (targetUIPanel != null)
        {
            targetUIPanel.SetActive(false); 
        }
        if (openButton != null)
        {
            openButton.SetActive(true); 
        }
    }
}