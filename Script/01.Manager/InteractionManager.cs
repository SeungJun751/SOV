using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private List<IInteractable> interactables = new List<IInteractable>();
    private IInteractable currentInteractable;

    public Transform player;
    public float interactionRange = 2f;

    public GameObject interactionPanel;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("⚠ 중복 InteractionManager 발견 → 삭제");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogWarning("⚠ Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        if (interactionPanel == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.name == "InteractionPanel")
                {
                    interactionPanel = obj;
                    interactionPanel.SetActive(false); 
                    Debug.Log("✅ 비활성 포함 InteractionPanel 자동 연결 완료!");
                    break;
                }
            }

            if (interactionPanel == null)
            {
                Debug.LogWarning("⚠ 'InteractionPanel'을 어디서도 찾을 수 없습니다.");
            }
        }

    }

    private void Update()
    {
        UpdateClosestInteractable();

        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void UpdateClosestInteractable()
    {
        if (player == null) return;

        float closestDistance = interactionRange;
        IInteractable closest = null;

        List<IInteractable> toRemove = new List<IInteractable>();

        foreach (var interactable in interactables)
        {
            if (interactable == null)
            {
                toRemove.Add(interactable);
                continue;
            }

            try
            {
                float distance = Vector3.Distance(player.position, interactable.GetPosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
            catch (MissingReferenceException)
            {
                if (!toRemove.Contains(interactable))
                {
                    Debug.LogWarning($"[InteractionManager] ❌ 파괴된 인터랙터 제거: {interactable}");
                    toRemove.Add(interactable);
                }
            }
        }

        foreach (var item in toRemove)
        {
            interactables.Remove(item);
        }

        if (closest != currentInteractable)
        {
            currentInteractable = closest;
            SetInteractionPanel(currentInteractable != null);
        }
    }


    public bool IsNear(Vector3 targetPosition)
    {
        if (player == null) return false;
        return Vector3.Distance(player.position, targetPosition) < interactionRange;
    }

    public void RegisterInteractable(IInteractable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);
        }
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactables.Contains(interactable))
        {
            interactables.Remove(interactable);
        }
    }

    public void SetInteractionPanel(bool isActive)
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(isActive);
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoRegisterPanel();
    }
    private void TryAutoRegisterPanel()
    {
        if (interactionPanel == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == "InteractionPanel")
                {
                    interactionPanel = obj;
                    interactionPanel.SetActive(false);
                    Debug.Log($"✅ InteractionPanel 씬 '{SceneManager.GetActiveScene().name}'에서 자동 연결됨!");
                    break;
                }
            }
        }
    }
}
