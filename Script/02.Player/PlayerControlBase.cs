// PlayerControlBase.cs (에러 방지 완전본)

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerControlBase : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 5f;
    private bool isGrounded;
    private Rigidbody rb;

    public Camera firstPersonCamera;
    private AudioListener firstPersonAudioListener;

    public Transform respawnPoint;
    public float respawnDelay = 0.5f;

    public GameObject waterEffectPrefab;
    public Transform rightHandTransform;

    private GameObject activeEffect;

    private bool isUnderwater = false;
    [SerializeField] private float sinkForce = 0.5f;
    [SerializeField] private float swimJumpForce = 4f;
    [SerializeField] private float swimJumpCooldown = 1.5f;

    private float nextSwimJumpTime = 0f;
    private bool jumpButtonReleased = true;
    private bool showEnterWaterMessage = false;
    [SerializeField] private float messageDuration = 3f; 
    private float messageTimer = 0f;
    private bool isInSeeTriggerZone = false;
    private float messageAnimY = 0f;
    private float messageAlpha = 0f;
    private float messageAnimSpeed = 5f;
    private string currentDisplayMessage = "";

    private bool canMove = true; 

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        firstPersonAudioListener = firstPersonCamera.GetComponent<AudioListener>();

        if (respawnPoint == null)
        {
            GameObject foundRespawn = GameObject.Find("RespawnPoint");
            if (foundRespawn != null)
            {
                respawnPoint = foundRespawn.transform;
                Debug.Log("🟢 RespawnPoint 자동 등록 완료");
            }
            else
            {
                Debug.LogWarning("⚠️ RespawnPoint를 씬에서 찾을 수 없습니다.");
            }
        }

        EnableFirstPersonCamera();
    }

    protected virtual void Update()
    {
        if (this == null || gameObject == null)
            return;

        if (canMove && (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null))
        {
            MovePlayer();
            Jump();
            if (isUnderwater)
            {
                if (jumpButtonReleased && Time.time >= nextSwimJumpTime && Input.GetButtonDown("Jump"))
                {
                    rb.AddForce(Vector3.up * swimJumpForce, ForceMode.Impulse);
                    nextSwimJumpTime = Time.time + swimJumpCooldown;
                    jumpButtonReleased = false;
                }

                if (Input.GetButtonUp("Jump"))
                {
                    jumpButtonReleased = true;
                }
            }
        }

        if (CompareTag(Tags.Player))
        {
            if (CanHandleWaterEffect())
                HandleWaterEffect();
        }

        if (showEnterWaterMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                showEnterWaterMessage = false;
                currentDisplayMessage = ""; 
            }
            float targetY = Screen.height * 0.08f;
            messageAnimY = Mathf.Lerp(messageAnimY, targetY, Time.deltaTime * messageAnimSpeed);
            messageAlpha = Mathf.MoveTowards(messageAlpha, 1f, Time.deltaTime * 2f);
        }
        else
        {
            messageAlpha = Mathf.MoveTowards(messageAlpha, 0f, Time.deltaTime * 2f); 
            {
                messageAnimY = 0f;
            }
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

    private void FixedUpdate()
    {
        if (isUnderwater)
        {
            rb.AddForce(Vector3.down * sinkForce, ForceMode.Acceleration);
        }
    }


    private bool CanHandleWaterEffect()
    {
        if (waterEffectPrefab == null)
        {
            return false;
        }
        if (rightHandTransform == null)
        {
            return false;
        }
        return true;
    }

    private void HandleWaterEffect()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (activeEffect == null)
            {
                activeEffect = Instantiate(waterEffectPrefab, rightHandTransform.position, Quaternion.identity);
                activeEffect.transform.SetParent(rightHandTransform);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (activeEffect != null)
            {
                Destroy(activeEffect);
                activeEffect = null;
            }
        }
    }

    private void MovePlayer()
    {
        if (!canMove) return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float moveUpDown = 0f;

        if (isUnderwater)
        {
            if (Input.GetKey(KeyCode.Space))
                moveUpDown = 1f;
            else if (Input.GetKey(KeyCode.LeftControl))
                moveUpDown = -1f;
        }

        Vector3 movement;
        if (isUnderwater)
        {
            movement = new Vector3(moveHorizontal, moveUpDown, moveVertical);
        }
        else
        {
            movement = new Vector3(moveHorizontal, 0f, moveVertical);
        }

        movement = transform.TransformDirection(movement);
        rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);
    }


    private void Jump()
    {
        if (!canMove) return;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Ground))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Water))
        {
            StartCoroutine(Respawn());
        }
        if (other.CompareTag("See"))
        {
            isInSeeTriggerZone = true;

            SetDisplayMessage("바다속을 들어가보세요.", messageDuration); 
            StartCoroutine(ShowMessageThenEnterUnderwater());
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Ground))
        {
            isGrounded = false;
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.position = respawnPoint.position;
        EnableFirstPersonCamera();
    }

    private void EnableFirstPersonCamera()
    {
        firstPersonCamera.enabled = true;
        firstPersonAudioListener.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("See"))
        {
            isInSeeTriggerZone = false;
            ExitUnderwaterMode();
        }
    }

    private void EnterUnderwaterMode()
    {
        isUnderwater = true;
        showEnterWaterMessage = false; 
        currentDisplayMessage = ""; 
        rb.useGravity = false;
    }

    private void ExitUnderwaterMode()
    {
        isUnderwater = false;
        rb.useGravity = true;

        RenderSettings.fog = false;
    }

    private void OnGUI()
    {
        if (showEnterWaterMessage && messageAlpha > 0f && !string.IsNullOrEmpty(currentDisplayMessage)) // ✅ currentDisplayMessage가 비어있지 않을 때만 표시
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 42;
            style.alignment = TextAnchor.UpperCenter;

            style.normal.textColor = new Color(1f, 1f, 1f, messageAlpha);

            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = new Color(0f, 0f, 0f, messageAlpha);

            Rect labelRect = new Rect(Screen.width / 2 - 300, messageAnimY, 600, 60);
            Rect shadowRect = new Rect(labelRect.x + 2, labelRect.y + 2, labelRect.width, labelRect.height);

            GUI.Label(shadowRect, currentDisplayMessage, shadowStyle); 
            GUI.Label(labelRect, currentDisplayMessage, style); 
        }
    }

    private IEnumerator ShowMessageThenEnterUnderwater()
    {
        messageTimer = messageDuration;

        yield return new WaitForSeconds(messageDuration);

        if (isInSeeTriggerZone) 
        {
            EnterUnderwaterMode();
        }

        showEnterWaterMessage = false; 
        currentDisplayMessage = "";
    }
    public void SetDisplayMessage(string message, float duration)
    {
        currentDisplayMessage = message;
        messageDuration = duration; 
        messageTimer = duration;
        showEnterWaterMessage = true;
        messageAnimY = 0f; 
        messageAlpha = 0f; 
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
        Debug.Log($"[PlayerControlBase] 플레이어 움직임 상태: {canMove}");
    }
}