using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 normalScale = Vector3.one;
    public Vector3 highlightedScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float scaleSpeed = 5f;

    private bool isHovered = false;
    private bool isResetting = false;

    void Start()
    {
        transform.localScale = normalScale;
    }

    void Update()
    {
        if (isHovered)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, highlightedScale, Time.deltaTime * scaleSpeed);
        }
        else if (isResetting)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, normalScale, Time.deltaTime * scaleSpeed);
            if (Vector3.Distance(transform.localScale, normalScale) < 0.01f)
            {
                isResetting = false; 
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isResetting = true;
    }

    public void ResetScale()
    {
        isHovered = false;
        isResetting = true;
    }
}
