using UnityEngine;
using UnityEngine.UI; 
using TMPro;      

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mode Settings")]
    [Tooltip("체크하면 센서 없이 자동으로 움직입니다.")]
    public bool isAutoMode = false; 
    [Tooltip("오토 모드일 때의 속도")]
    public float autoModeSpeed = 3.0f; 

    [Header("Manual Movement Settings")]
    public float maxSpeed = 5f;
    public float speedBoostPerCount = 2f;
    public float deceleration = 1f;

    [Header("Waypoint Settings")]
    [Tooltip("이동할 경로")]
    public Transform[] waypoints;
    [Tooltip("이 거리 안으로 들어오면 다음 웨이포인트로 넘어갑니다.")]
    public float arrivalDistance = 1.0f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public float lookAheadDistance = 5f;

    [Header("Footstep Settings")]
    public FootstepManager footstepManager;
    [Tooltip("오토 모드일 때 발소리가 나는 시간 간격")]
    public float autoFootstepInterval = 0.5f; 

    [Header("UI Settings (Options Window)")]
    [Tooltip("현재 최대 속도를 표시할 텍스트 (TMP)")]
    public TextMeshProUGUI maxSpeedText;
    [Tooltip("속도 감소 버튼")]
    public Button decreaseButton;
    [Tooltip("속도 증가 버튼")]
    public Button increaseButton;

    private const float minSpeed = 3f;
    private const float maxSpeedLimit = 12f; 

    private Rigidbody rb;
    private int lastReceivedCount = -1;
    private float footstepTimer = 0f; 

    public int currentWaypointIndex { get; private set; } = 0;
    public float currentSpeed { get; private set; } = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        lastReceivedCount = FirebaseDataReceiver.CurrentCount;
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
        UpdateSpeedUI(this.maxSpeed);
    }

    void Update()
    {
        if (isAutoMode)
        {
            HandleAutoMode();
        }
        else
        {
            HandleSensorMode();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance != null && UIManager.Instance.optionsWindow != null && UIManager.Instance.optionsWindow.activeSelf)
            {
                UIManager.Instance.CloseOptionsMenu();
            }
            else
            {
                UIManager.Instance.OpenOptionsMenu();
            }
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleSensorMode()
    {
        int newCount = FirebaseDataReceiver.CurrentCount;
        if (newCount > lastReceivedCount)
        {
            int steps = newCount - lastReceivedCount;
            if (footstepManager != null)
            {
                for (int i = 0; i < steps; i++)
                {
                    footstepManager.PlayFootstepSound();
                }
            }
            currentSpeed = Mathf.Min(currentSpeed + (steps * speedBoostPerCount), maxSpeed);
            lastReceivedCount = newCount;
        }
        currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
    }

    private void HandleAutoMode()
    {
        if (currentWaypointIndex < waypoints.Length - 1)
        {
            currentSpeed = autoModeSpeed;

            if (footstepManager != null)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    footstepManager.PlayFootstepSound();
                    footstepTimer = autoFootstepInterval;
                }
            }
        }
        else 
        {
            currentSpeed = 0f;
        }
    }

    private void HandleMovement()
    {
        if (currentSpeed > 0 && waypoints.Length > 1 && currentWaypointIndex < waypoints.Length - 1)
        {
            Vector3 targetWaypoint = waypoints[currentWaypointIndex + 1].position;
            Vector3 positionOnPlane = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetOnPlane = new Vector3(targetWaypoint.x, 0, targetWaypoint.z);
            float distanceToTarget = Vector3.Distance(positionOnPlane, targetOnPlane);
            if (distanceToTarget < arrivalDistance)
            {
                currentWaypointIndex++;
            }
            else 
            {
                Vector3 direction = (targetOnPlane - positionOnPlane).normalized;
                Vector3 targetPosition = rb.position + direction * currentSpeed * Time.fixedDeltaTime;
                rb.MovePosition(targetPosition);
            }
        }
    }

    private void HandleRotation()
    {
        if (waypoints.Length < 2) return;
        Vector3 lookAheadPoint = GetLookAheadPosition();
        Vector3 directionToLook = lookAheadPoint - transform.position;
        directionToLook.y = 0;
        if (directionToLook != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }

    private Vector3 GetLookAheadPosition()
    {
        Vector3 position = transform.position;
        float remainingLookAhead = lookAheadDistance;
        int tempWaypointIndex = currentWaypointIndex;
        while (remainingLookAhead > 0)
        {
            if (tempWaypointIndex >= waypoints.Length - 1) return waypoints[waypoints.Length - 1].position;
            Vector3 nextWaypoint = waypoints[tempWaypointIndex + 1].position;
            float distanceToNextWaypoint = Vector3.Distance(new Vector3(position.x, 0, position.z), new Vector3(nextWaypoint.x, 0, nextWaypoint.z));
            if (distanceToNextWaypoint <= 0) { tempWaypointIndex++; continue; }
            if (remainingLookAhead <= distanceToNextWaypoint) return position + (nextWaypoint - position).normalized * remainingLookAhead;
            else { remainingLookAhead -= distanceToNextWaypoint; position = nextWaypoint; tempWaypointIndex++; }
        }
        return waypoints[waypoints.Length - 1].position;
    }
    public void DecreaseMaxSpeed()
    {
        float newSpeed = this.maxSpeed - 1f;
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeedLimit);

        this.maxSpeed = newSpeed;
        UpdateSpeedUI(newSpeed);
    }

    public void IncreaseMaxSpeed()
    {
        float newSpeed = this.maxSpeed + 1f;
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeedLimit);

        this.maxSpeed = newSpeed; 
        UpdateSpeedUI(newSpeed);
    }

    private void UpdateSpeedUI(float speed)
    {
        if (maxSpeedText != null)
        {
            maxSpeedText.text = speed.ToString("F0");
        }

        if (decreaseButton != null)
        {
            decreaseButton.interactable = (speed > minSpeed);
        }
        if (increaseButton != null)
        {
            increaseButton.interactable = (speed < maxSpeedLimit);
        }
    }
}