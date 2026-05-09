using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!
using UnityEngine.UI;

public class GameClearManager : MonoBehaviour
{
    [Header("게임 클리어 조건")]
    public PlayerController playerController;

    public GameObject clearPanel;

    [Header("결과 표시")]
    public PlaytimeManager playtimeManager;
    public Text finalPlaytimeText;

    [Header("씬 전환 설정")]
    public string mainMenuSceneName = "MainScene"; 

    private bool isCleared = false; 

    void Update()
    {
        if (!isCleared && playerController != null && playerController.currentWaypointIndex >= playerController.waypoints.Length - 1)
        {
            GameClear();
        }
    }

    void GameClear()
    {
        isCleared = true; 
        playerController.enabled = false;
        playtimeManager.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (finalPlaytimeText != null && playtimeManager != null)
        {
            finalPlaytimeText.text = "플레이 시간: " + playtimeManager.playtimeText.text;
        }

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
    }

    public void GoToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}