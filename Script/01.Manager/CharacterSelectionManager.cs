using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    public GameObject characterSelectionPanel;

    public List<CharacterData> allCharacters;

    private int selectedCharacterIndex = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(int index, string sceneToLoad = "Scene01")
    {
        selectedCharacterIndex = index;
        characterSelectionPanel?.SetActive(false); 

        LoadingManager loadingManager = FindFirstObjectByType<LoadingManager>();
        if (loadingManager != null)
        {
            loadingManager.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void SelectCharacter(int index)
    {
        SelectCharacter(index, "Scene01"); 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (selectedCharacterIndex < 0 || selectedCharacterIndex >= allCharacters.Count) return;

        CharacterData selectedData = allCharacters[selectedCharacterIndex];

        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null && selectedData.characterPrefab != null)
        {
            GameObject character = Instantiate(selectedData.characterPrefab, spawn.transform.position, Quaternion.identity);
            InteractionManager.Instance?.SetPlayer(character.transform);
        }

        AudioManager.Instance?.ResetAudioMixerAndApplyVolume();

        AudioClip clipToPlay = null;
        foreach (var sceneBgm in selectedData.sceneBGMs)
        {
            if (sceneBgm.sceneName == scene.name)
            {
                clipToPlay = sceneBgm.bgmClip;
                break;
            }
        }

        if (clipToPlay != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"[BGM] 캐릭터 '{selectedData.characterName}'의 씬 '{scene.name}'에 맞는 BGM이 등록되지 않았습니다.");
            AudioManager.Instance?.StopBGM();
        }
    }

    public void ResetSelection()
    {
        selectedCharacterIndex = -1;
    }
    public bool IsCharacterSelected()
    {
        return selectedCharacterIndex != -1;
    }

    public CharacterData GetSelectedCharacterData()
    {
        if (IsCharacterSelected() && selectedCharacterIndex < allCharacters.Count)
        {
            return allCharacters[selectedCharacterIndex];
        }
        return null;
    }

    public AudioClip GetBGMForCurrentScene()
    {
        CharacterData selectedData = GetSelectedCharacterData();
        if (selectedData == null) return null;

        string currentSceneName = SceneManager.GetActiveScene().name;
        foreach (var sceneBgm in selectedData.sceneBGMs)
        {
            if (sceneBgm.sceneName == currentSceneName)
            {
                return sceneBgm.bgmClip;
            }
        }
        return null;
    }
}