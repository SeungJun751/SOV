using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하려면 이 줄이 필수입니다!

public class ProgressManager : MonoBehaviour
{
    public PlayerController playerController;
    public Slider progressBar;

    private float totalPathLength;
    private Transform[] waypoints;

    void Start()
    {
        if (playerController == null || playerController.waypoints.Length < 2)
        {
            Debug.LogError("ProgressManager: PlayerController 또는 Waypoints가 설정되지 않았습니다!");
            this.enabled = false;
            return;
        }

        waypoints = playerController.waypoints;
        CalculateTotalPathLength();
    }

    void Update()
    {
        UpdateProgressBar();
    }

    void CalculateTotalPathLength()
    {
        totalPathLength = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            totalPathLength += Vector3.Distance(
                new Vector3(waypoints[i].position.x, 0, waypoints[i].position.z),
                new Vector3(waypoints[i + 1].position.x, 0, waypoints[i + 1].position.z)
            );
        }
    }

    void UpdateProgressBar()
    {
        if (totalPathLength <= 0) return;
        float distanceTraveled = 0f;
        for (int i = 0; i < playerController.currentWaypointIndex; i++)
        {
            distanceTraveled += Vector3.Distance(
                new Vector3(waypoints[i].position.x, 0, waypoints[i].position.z),
                new Vector3(waypoints[i + 1].position.x, 0, waypoints[i + 1].position.z)
            );
        }
        if (playerController.currentWaypointIndex < waypoints.Length)
        {
            distanceTraveled += Vector3.Distance(
                new Vector3(waypoints[playerController.currentWaypointIndex].position.x, 0, waypoints[playerController.currentWaypointIndex].position.z),
                new Vector3(playerController.transform.position.x, 0, playerController.transform.position.z)
            );
        }

        float progress = Mathf.Clamp01(distanceTraveled / totalPathLength);

        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }
}