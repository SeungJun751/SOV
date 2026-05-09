using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    public RawImage RawImage;


    public GameObject mainMenuUI;
    public GameObject characterSelectionPanel;
    public Button loadGameButton;        
    public GameObject loadGamePanel;      

    [Header("Loading Management")]
    public LoadingManager loadingManager;


    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        startButton.onClick.AddListener(StartGame);

 
        loadGameButton.onClick.AddListener(OpenLoadGamePanel);

        RawImage.enabled = true;

        mainMenuUI.SetActive(true);
        characterSelectionPanel.SetActive(false);

        loadGamePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartGame();
        }
    }

    public void OpenLoadGamePanel()
    {
        mainMenuUI.SetActive(false);
        loadGamePanel.SetActive(true);
    }

    public void CloseLoadGamePanel()
    {
        loadGamePanel.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void CloseCharacterSelect()
    {
        characterSelectionPanel.SetActive(false);
        mainMenuUI.SetActive(true);
        RawImage.enabled = true;
    }

    void StartGame()
    {
        CharacterSelectionManager.Instance.ResetSelection();
        mainMenuUI.SetActive(false);
        RawImage.enabled = false;
        characterSelectionPanel.SetActive(true);
    }
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        if (loadingManager != null)
        {
            loadingManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("MainMenu 스크립트에 LoadingManager가 연결되지 않았습니다!");
        }
    }
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨");
    }
}