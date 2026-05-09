using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Intro : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject panelBackground;
    [SerializeField] private GameObject lightEffect;

    [Header("플레이어 제어 스크립트")]
    [SerializeField] private MonoBehaviour playerController; 

    [Header("인트로 문장")]
    [TextArea(2, 5)]
    [SerializeField]
    private string[] introLines = new string[]
    {
        "그들은 결국 모든 걸 가져갔어.",
        "동쪽에서 온 바람이 옆을 스쳐가고,\n새들은 모여 합창을 하고\n땅과 하늘, 그리고 바다가\n하나의 선율로 화합을 이루던 날들…",
        "그 소리들은, 세상의 심장처럼 뛰고 있었지.",
        "하지만 이젠… 아무것도 들리지 않아.",
        "홀로 이 모든 걸 짊어지게 해서…\n정말 미안해.",
        "봉인된 소리를 모두 해방시켜줘\n너는, 세상의 마지막 기억이자\n다시 울려 퍼질 첫 번째 소리야.",
        "부디… 잊힌 것들을 깨워줘"
    };

    private int currentLine = 0;
    private bool isTyping = false;
    private bool introEnded = false;

    public static bool IsIntroPlaying { get; private set; }
    private CameraController cameraController;

    void Start()
    {
        IsIntroPlaying = true;
        nextButton.gameObject.SetActive(false);
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerControl>();
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerController != null)
            playerController.enabled = false;

        cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
            cameraController.enabled = false;

        if (lightEffect != null)
            lightEffect.SetActive(false);
        if (panelBackground != null)
            panelBackground.SetActive(true);

        nextButton?.onClick.AddListener(OnNextClicked);
        skipButton?.onClick.AddListener(OnSkipClicked);

        StartCoroutine(TypeSentence(introLines[currentLine]));
    }

    void Update()
    {
        if (!introEnded)
        {
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;

            if (!Cursor.visible)
                Cursor.visible = true;
        }
    }

    void OnNextClicked()
    {
        if (isTyping) return;

        nextButton.gameObject.SetActive(false);

        currentLine++;
        if (currentLine < introLines.Length)
        {
            StartCoroutine(TypeSentence(introLines[currentLine]));
        }
        else
        {
            OnSkipClicked();
        }
    }

    void OnSkipClicked()
    {
        StopAllCoroutines();            
        isTyping = false;

        if (panelBackground != null) panelBackground.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        if (cameraController != null)
            cameraController.enabled = true;

        StartCoroutine(DeferredFinalSteps());

        IsIntroPlaying = false;
        introEnded = true;

        EventSystem.current.SetSelectedGameObject(null);

        if (lightEffect != null)
        {
            lightEffect.SetActive(true);
            AudioManager.Instance?.PlaySFX("LightFlash");
        }
    }


    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in sentence)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f); 
        }

        isTyping = false;
        nextButton.gameObject.SetActive(true);
    }
    IEnumerator DeferredFinalSteps()
    {
        yield return new WaitForSeconds(0.1f); 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);
    }
}
