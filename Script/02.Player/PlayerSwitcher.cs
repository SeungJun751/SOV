using UnityEngine;

public class PlayerSwitcher : MonoBehaviour
{
    public GameObject[] playerPrefabs; 
    private int currentIndex = 0;       
    private GameObject currentPlayer; 

    private bool canSwitch = false;     
    private WaterPuzzle waterPuzzle; 

    public Transform respawnPoint; 


    void Start()
    {
        waterPuzzle = FindFirstObjectByType<WaterPuzzle>();

        if (playerPrefabs.Length > 0)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            SpawnPlayer(0, startPos, startRot);
        }
    }

    void Update()
    {
        if (canSwitch && Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchPlayer();
        }
    }

    void SpawnPlayer(int index, Vector3 position, Quaternion rotation)
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        currentPlayer = Instantiate(playerPrefabs[index], position, rotation);
        currentIndex = index;

        PlayerControl playerControl = currentPlayer.GetComponent<PlayerControl>();
        if (playerControl != null)
        {
            playerControl.respawnPoint = respawnPoint; 
        }

        PlayerControlBase playerControlBase = currentPlayer.GetComponent<PlayerControlBase>();
        if (playerControlBase != null)
        {
            playerControlBase.respawnPoint = respawnPoint; 
        }

        if (waterPuzzle != null)
        {
            waterPuzzle.SetAllowedPlayer(currentPlayer);
        }
    }




    void SwitchPlayer()
    {
        if (currentPlayer == null) return;

        Vector3 currentPos = currentPlayer.transform.position;
        Quaternion currentRot = currentPlayer.transform.rotation;

        int nextIndex = (currentIndex + 1) % playerPrefabs.Length;
        SpawnPlayer(nextIndex, currentPos, currentRot);
    }

    public void EnableSwitch()
    {
        canSwitch = true;
        Debug.Log("🧜‍♀️ 캐릭터 스위치 활성화!");
    }
}
