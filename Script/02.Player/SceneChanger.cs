using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public string nextSceneName;

    [SerializeField] private bool requirePuzzle1;
    [SerializeField] private bool requireMelodyPuzzle;
    [SerializeField] private bool requireAnimalPuzzle;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Player))
        {
            if (requirePuzzle1 && !Puzzle1.PuzzleCleared)
            {
                Debug.Log("⛔ 퍼즐1을 완료해야 이동할 수 있습니다.");
                return;
            }
            if (requireMelodyPuzzle && !MelodyPuzzleManager.PuzzleCleared)
            {
                Debug.Log("⛔ 멜로디 퍼즐을 완료해야 이동할 수 있습니다.");
                return;
            }
            if (requireAnimalPuzzle && !PlayerLineDrawer.AnimalPuzzleCleared)
            {
                Debug.Log("⛔ 동물 소리 퍼즐을 완료해야 이동할 수 있습니다.");
                return;
            }

            Debug.Log($"▶ {nextSceneName} 씬 로딩 시작!");
            LoadingManager loadingManager = FindFirstObjectByType<LoadingManager>();

            if (loadingManager != null)
            {
                loadingManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("⚠ LoadingManager 인스턴스를 찾을 수 없습니다. 그냥 씬 전환합니다.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
