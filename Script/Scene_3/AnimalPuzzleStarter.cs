using System.Collections;
using UnityEngine;

public class AnimalPuzzleStarter : MonoBehaviour
{
    [Header("🐾 퍼즐 시작 조건")]
    public Transform player; 
    public AudioClip roarClip; 
    public Transform animalStartPoint;
    public GameObject animalGhostPrefab; 

    [Header("🎥 카메라 전환")]
    public GameObject cinematicCam; 
    public GameObject playerCam;     
    public float camDuration = 4f; 

    [Header("🌳 퍼즐 관리자")]
    public AnimalSoundTrailPuzzle puzzleManager; 
    public Transform puzzleStartTree; 
    public PlayerLineDrawer playerLineDrawer; 
    private PlayerControlBase playerControlBase; 

    private bool isRetryingPuzzle = false;
    private bool triggered = false; 

    private float originalBgmVolume;

    void Start()
    {
        if (player == null) 
        {
            GameObject playerObj = GameObject.FindWithTag("Player"); 
            if (playerObj != null)
            {
                player = playerObj.transform; 
                Debug.Log("[AnimalPuzzleStarter] 'Player' 태그를 가진 플레이어 오브젝트 자동 연결 완료.");
            }
            else
            {
                Debug.LogWarning("[AnimalPuzzleStarter] 'Player' 태그를 가진 오브젝트를 씬에서 찾을 수 없습니다. 수동 할당이 필요할 수 있습니다.");
            }
        }

        GameObject playerCamObject = GameObject.FindWithTag("PlayerCamera"); 
        if (playerCamObject != null) 
        {
            playerControlBase = playerCamObject.GetComponentInParent<PlayerControlBase>(); 
            if (playerControlBase == null) 
            {
                playerControlBase = playerCamObject.GetComponent<PlayerControlBase>(); 
            }
            if (playerControlBase == null) 
            {
                Debug.LogWarning("[AnimalPuzzleStarter] PlayerCamera에서 PlayerControlBase 스크립트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[AnimalPuzzleStarter] 'PlayerCamera' 태그를 가진 오브젝트를 씬에서 찾을 수 없습니다. PlayerControlBase 연결에 문제가 있을 수 있습니다.");
        }

        StartCoroutine(AssignPlayerCameraWhenReady()); 
    }

    IEnumerator AssignPlayerCameraWhenReady() 
    {
        while (playerCam == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player"); 
            if (playerObj != null) 
            {
                Camera foundCam = playerObj.GetComponentInChildren<Camera>(); 
                if (foundCam != null) 
                {
                    playerCam = foundCam.gameObject; 
                    Debug.Log("📸 플레이어 카메라 자동 연결 완료"); 
                    yield break; 
                }
            }
            yield return null; 
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (triggered || !other.CompareTag("Player")) return; 
        triggered = true; 

        StartCoroutine(PlayCinematicSequence()); 
    }
    public void RetryPuzzleSequence()
    {
        if (isRetryingPuzzle) return;
        isRetryingPuzzle = true; 
        Debug.Log("[AnimalPuzzleStarter] 퍼즐 시퀀스 재시작 요청됨.");
        StartCoroutine(PlayCinematicSequence(true)); 
    }

    private IEnumerator PlayCinematicSequence(bool isRetry = false) 
    {
        if (!isRetry) 
        {
            triggered = true; 
        }
        else
        {
            if (puzzleManager != null)
            {
                if (puzzleManager.trailRoutine != null) 
                {
                    StopCoroutine(puzzleManager.trailRoutine); 
                    puzzleManager.trailRoutine = null; 
                }
            }
            if (playerControlBase != null) 
            {
                playerControlBase.SetCanMove(false); 
            }
        }

        if (player != null)
        {
            player.gameObject.SetActive(false);
            Debug.Log("[AnimalPuzzleStarter] 플레이어 캐릭터 숨김");
        }

        AudioSource.PlayClipAtPoint(roarClip, animalStartPoint.position);
        yield return new WaitForSeconds(0.5f);

        if (playerControlBase != null)
        {
            playerControlBase.SetCanMove(false);
        }

        if (playerCam != null)
        {
            playerCam.SetActive(false);
            var listener = playerCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        if (cinematicCam != null)
        {
            cinematicCam.SetActive(true);
            var listener = cinematicCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
        }
        if (AudioManager.Instance != null)
        {
            originalBgmVolume = AudioManager.Instance.bgmSource.volume; 
            AudioManager.Instance.SetBGMVolume(0f); 
            Debug.Log($"🔊 BGM 볼륨을 0으로 변경. (원래 볼륨: {originalBgmVolume})");
        }
        GameObject ghost = Instantiate(animalGhostPrefab, animalStartPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(2.5f);
        Destroy(ghost);

        yield return new WaitForSeconds(0.5f);

        if (puzzleManager != null)
        {
            if (playerLineDrawer != null)
            {
                playerLineDrawer.animalPuzzle = puzzleManager;
                playerLineDrawer.allTrees = puzzleManager.treePositions;
            }

            puzzleManager.StartPuzzleFrom(puzzleStartTree.position);

            while (puzzleManager.GetSoundPattern().Count > 0 && puzzleManager.trailRoutine != null)
            {
                yield return null;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(originalBgmVolume); 
                Debug.Log($"🔊 BGM 볼륨을 원래대로 복구: {originalBgmVolume}");
            }

            if (playerLineDrawer != null)
                playerLineDrawer.SetCorrectPath(puzzleManager.GetSoundPattern());
        }
        if (cinematicCam != null)
        {
            cinematicCam.SetActive(false);
            var listener = cinematicCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        if (playerCam != null)
        {
            playerCam.SetActive(true);
            var listener = playerCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
        }

        yield return new WaitForSeconds(0.2f); 

        if (playerControlBase != null)
        {
            playerControlBase.SetCanMove(true);
        }
        if (player != null)
        {
            player.gameObject.SetActive(true);
            Debug.Log("[AnimalPuzzleStarter] 플레이어 캐릭터 다시 나타남");
        }

        isRetryingPuzzle = false; 
    }
}