using UnityEngine;
using UnityEngine.EventSystems;

public class WaterPuzzle : MonoBehaviour
{
    [Header("설정")]
    public float maxDistance = 10f;
    public float changeSpeed = 1.0f;
    public LayerMask waterLayer;
    public float minHeight = -78f; 
    public float maxHeight = -76f; 

    [Header("플레이어")]
    public GameObject allowedPlayer;

    private Camera playerCamera;
    private PlayerControlBase playerControlBase;
    private WaterSound lastAdjustedWater = null;

    void Start()
    {
        InitializePlayerReferences();
    }

    void Update()
    {
        if (allowedPlayer == null || !allowedPlayer.activeInHierarchy)
        {
            InitializePlayerReferences();
            if (allowedPlayer == null) return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (playerCamera == null) return;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, waterLayer))
            {
                AdjustWaterHeight(hit.transform);
                lastAdjustedWater = hit.transform.GetComponent<WaterSound>();
            }
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && lastAdjustedWater != null)
        {
            if (playerControlBase != null)
            {
                int noteIndex = lastAdjustedWater.GetCurrentNoteIndex();
                string[] noteNames = { "도", "레", "미", "파", "솔", "라", "시", "도(높은)" };

                if (noteIndex >= 0 && noteIndex < noteNames.Length)
                {
                    playerControlBase.SetDisplayMessage($"현재 음: {noteNames[noteIndex]}", 2f);
                }
            }
            lastAdjustedWater = null;
        }
    }

    void AdjustWaterHeight(Transform waterTransform)
    {
        float direction = Input.GetMouseButton(1) ? 1f : -1f; 
        Vector3 newPosition = waterTransform.position + Vector3.up * direction * changeSpeed * Time.deltaTime;

        newPosition.y = Mathf.Clamp(newPosition.y, minHeight, maxHeight);

        waterTransform.position = newPosition;

        WaterSound ws = waterTransform.GetComponent<WaterSound>();
        if (ws != null)
        {
            ws.PlayNoteSound(ws.GetCurrentNoteIndex());
        }
    }

    public void SetAllowedPlayer(GameObject player)
    {
        allowedPlayer = player;
        InitializePlayerReferences();
    }

    private void InitializePlayerReferences()
    {
        if (allowedPlayer == null) allowedPlayer = GameObject.FindWithTag("Player");
        if (allowedPlayer != null)
        {
            playerControlBase = allowedPlayer.GetComponent<PlayerControlBase>();
            playerCamera = allowedPlayer.GetComponentInChildren<Camera>();
        }
    }
}