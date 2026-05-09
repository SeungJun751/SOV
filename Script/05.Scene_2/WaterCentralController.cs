using UnityEngine;

public class WaterCentralController : MonoBehaviour, IInteractable
{
    public MelodyPuzzleManager melodyPuzzleManager;
    private bool isChecking = false;
    public float interactDistance = 2.5f;
    private Transform playerTransform;

    public void Interact()
    {
        if (isChecking) return;
        if (melodyPuzzleManager != null)
        {
            isChecking = true; 

            Debug.Log("정답을 확인합니다...");
            melodyPuzzleManager.CheckAnswerExternally();

            isChecking = false; /
        }
        else
        {
            Debug.LogError("MelodyPuzzleManager가 할당되지 않았습니다!");
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    void Start()
    {
        InteractionManager.Instance?.RegisterInteractable(this);
    }

    void OnDestroy()
    {
        InteractionManager.Instance?.UnregisterInteractable(this);
    }
}