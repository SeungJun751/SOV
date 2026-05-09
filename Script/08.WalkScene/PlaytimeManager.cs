using UnityEngine;
using TMPro; 

public class PlaytimeManager : MonoBehaviour
{
    public TextMeshProUGUI playtimeText;

    private float elapsedTime = 0f;

    void Update()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        if (playtimeText != null)
        {
            playtimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}