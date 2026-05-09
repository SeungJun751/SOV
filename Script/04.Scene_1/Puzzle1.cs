using System.Collections;
using System.Collections.Generic;
using DoorScript;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Puzzle1 : MonoBehaviour, IInteractable
{
    public GameObject interactableObject; 
    public GameObject[] pillars; 
    public GameObject centerEffectPrefab; 
    public Transform centerPoint; 
    public string nextSceneName; 
    public GameObject sceneTransitionObject; 
    public Door door;

    public Camera puzzleCamera; 

    private int currentStep = 0;
    private List<int> soundOrder = new List<int>();
    private bool canInteract = false;
    private bool puzzleSolved = false;
    private static bool alreadyLoggedMissing = false;

    private Dictionary<int, AudioSource> pillarAudioSources = new Dictionary<int, AudioSource>();

    public static bool PuzzleCleared { get; private set; } = false;

    private Camera playerCamera; 
    private PlayerControlBase playerControlBase; 

    private void Awake()
    {
        AudioManager.Instance.SetBGMVolume(0f);
        AudioManager.Instance.PlayBGM(null);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.canPlayBGM = false;
            AudioManager.Instance.bgmSource.Stop(); 
        }
    }

    private IEnumerator Start()
    {
        AudioManager.Instance.SetBGMVolume(0f);
        AudioManager.Instance.PlayBGM(null);
        yield return null;

        GameObject playerCamObject = GameObject.FindWithTag("PlayerCamera");
        if (playerCamObject != null)
        {
            playerCamera = playerCamObject.GetComponent<Camera>();
            playerControlBase = playerCamObject.GetComponentInParent<PlayerControlBase>();
            if (playerControlBase == null)
            {
                playerControlBase = playerCamObject.GetComponent<PlayerControlBase>();
            }
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("⚠ Puzzle1: 'PlayerCamera' 태그를 가진 카메라를 찾을 수 없거나, 해당 오브젝트에 Camera 컴포넌트가 없습니다. 플레이어 카메라에 'PlayerCamera' 태그를 추가하고 Camera 컴포넌트가 있는지 확인해주세요.");
        }
        if (playerControlBase == null)
        {
            Debug.LogWarning("⚠ Puzzle1: 'PlayerCamera' 태그를 가진 오브젝트나 그 부모에서 PlayerControlBase 스크립트를 찾을 수 없습니다. 플레이어 움직임을 제어할 PlayerControlBase 스크립트가 올바르게 설정되어 있는지 확인해주세요.");
        }

        if (puzzleCamera != null)
        {
            puzzleCamera.gameObject.SetActive(false);
        }

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.RegisterInteractable(this);

            for (int i = 0; i < pillars.Length; i++)
            {
                var pillar = pillars[i];
                IInteractable interactable = pillar.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    InteractionManager.Instance.RegisterInteractable(interactable);
                }

                AudioSource pillarAudio = pillar.GetComponent<AudioSource>();
                if (pillarAudio == null)
                    pillarAudio = pillar.AddComponent<AudioSource>();

                pillarAudio.spatialBlend = 1f;
                pillarAudio.rolloffMode = AudioRolloffMode.Linear;
                pillarAudio.minDistance = 1f;
                pillarAudio.maxDistance = 10f;
                pillarAudio.playOnAwake = false;

                pillarAudioSources[i] = pillarAudio;
            }

            if (sceneTransitionObject != null)
            {
                IInteractable interactable = sceneTransitionObject.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    InteractionManager.Instance.RegisterInteractable(interactable);
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠ Puzzle1: InteractionManager.Instance가 null입니다. RegisterInteractable 실패.");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(0f);
            AudioManager.Instance.PlayBGM(null);
        }
    }


    public void Interact()
    {
        if (!puzzleSolved && !canInteract)
        {
            StartCoroutine(PlaySoundSequence());
        }
        else if (puzzleSolved && sceneTransitionObject != null
                 && InteractionManager.Instance.IsNear(sceneTransitionObject.transform.position))
        {
            LoadNextScene();
        }
    }

    public Vector3 GetPosition()
    {
        if (this == null)
        {
            if (!alreadyLoggedMissing)
            {
                Debug.LogWarning("[Puzzle1] this가 이미 파괴되었습니다.");
                alreadyLoggedMissing = true;
            }
            return Vector3.zero;
        }

        return transform.position;
    }



    private IEnumerator PlaySoundSequence()
    {
        canInteract = true;
        soundOrder.Clear();

        if (playerControlBase != null)
        {
            playerControlBase.SetCanMove(false);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }
        if (puzzleCamera != null)
        {
            puzzleCamera.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.5f);

        List<int> availablePillars = new List<int>();
        for (int i = 0; i < pillars.Length; i++)
        {
            availablePillars.Add(i);
        }

        for (int i = 0; i < pillars.Length; i++)
        {
            int randomIndex = Random.Range(0, availablePillars.Count);
            int selectedPillar = availablePillars[randomIndex];
            soundOrder.Add(selectedPillar);
            availablePillars.RemoveAt(randomIndex);
        }

        foreach (int index in soundOrder)
        {
            Debug.Log($"🎵 Playing 3D sound for pillar {index}");
            PlayPillarSound(index);
            yield return new WaitForSeconds(1f);
        }

        if (puzzleCamera != null)
        {
            puzzleCamera.gameObject.SetActive(false);
        }
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.5f); 

        if (playerControlBase != null)
        {
            playerControlBase.SetCanMove(true);
        }

        yield return StartCoroutine(WaitForPillarInteraction());
    }

    private void PlayPillarSound(int pillarIndex)
    {
        if (!pillarAudioSources.ContainsKey(pillarIndex)) return;

        AudioSource pillarAudio = pillarAudioSources[pillarIndex];
        AudioClip clip = AudioManager.Instance.GetPillarClip(pillarIndex);

        if (clip != null)
        {
            pillarAudio.clip = clip;
            pillarAudio.Play();
        }
        else
        {
            Debug.LogWarning($"🚨 [오류] 기둥 {pillarIndex}의 오디오 클립이 없습니다!");
        }
    }

    private IEnumerator WaitForPillarInteraction()
    {
        currentStep = 0;
        canInteract = true;

        while (currentStep < soundOrder.Count)
        {
            bool isNearAnyPillar = false;

            foreach (var pillar in pillars)
            {
                if (InteractionManager.Instance.IsNear(pillar.transform.position))
                {
                    isNearAnyPillar = true;

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        int pillarIndex = System.Array.IndexOf(pillars, pillar);

                        if (pillarIndex == soundOrder[currentStep])
                        {
                            Debug.Log($"✅ 올바른 기둥 선택: {pillarIndex}");
                            ActivateLight(pillar);
                            currentStep++;
                        }
                        else
                        {
                            Debug.Log("❌ 잘못된 기둥 선택! 퍼즐 리셋.");
                            ResetPillars();
                            yield break;
                        }
                    }
                }
            }

            InteractionManager.Instance.SetInteractionPanel(isNearAnyPillar);
            yield return null;
        }

        InteractionManager.Instance.SetInteractionPanel(false);
        SpawnEffect(centerEffectPrefab, centerPoint.position);
        TurnOffOtherPillarsLights();
        puzzleSolved = true;
        PuzzleCleared = true;

        yield return new WaitForSeconds(2f);

        if (door != null)
        {
            door.OpenDoor(); 
        }

        Debug.Log($"🎵 선택된 캐릭터 인덱스 유효 여부: {CharacterSelectionManager.Instance.IsCharacterSelected()}"); //

        AudioClip clip = CharacterSelectionManager.Instance.GetBGMForCurrentScene();
        if (clip != null)
        {
            AudioManager.Instance.canPlayBGM = true;
            AudioManager.Instance.SetBGMVolume(1f);
            AudioManager.Instance.PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning("❌ BGM 재생 실패: 선택된 캐릭터의 현재 씬 BGM을 찾을 수 없습니다.");
        }
    }

    private void ResetPillars()
    {
        foreach (var pillar in pillars)
        {
            Light light = pillar.GetComponent<Light>();
            if (light != null)
            {
                light.enabled = false;
            }
        }
        currentStep = 0;
        canInteract = false;
        InteractionManager.Instance.SetInteractionPanel(false);

        if (puzzleCamera != null)
        {
            puzzleCamera.gameObject.SetActive(false);
        }
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
        if (playerControlBase != null)
        {
            playerControlBase.SetCanMove(true);
        }

        Debug.Log("🔄 퍼즐 리셋: 잘못된 선택으로 인해 다시 시작.");
    }

    private void ActivateLight(GameObject pillar)
    {
        Light light = pillar.GetComponent<Light>();
        if (light == null)
        {
            light = pillar.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 3f;
            light.color = Color.yellow;
            light.shadows = LightShadows.Soft;
        }
        light.enabled = true;
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
    }

    private void TurnOffOtherPillarsLights()
    {
        foreach (var pillar in pillars)
        {
            Light light = pillar.GetComponent<Light>();
            if (light != null && light.enabled)
            {
                light.enabled = false;
            }
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"▶ 씬 전환: {nextSceneName} 로 이동합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}