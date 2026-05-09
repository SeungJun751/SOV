using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource; 
    public AudioSource sfxSource; 

    public List<AudioClip> sfxClips; /
    public AudioClip defaultBGM;
    private Dictionary<string, AudioClip> sfxDictionary; 

    public AudioClip buttonClickSFX;
    public AudioClip[] pillarClips; 

    public AudioMixer audioMixer; 
    [Header("Mixer Groups")]
    public AudioMixerGroup bgmGroup;
    public bool canPlayBGM { get; set; } = true;

    private AudioClip lastPlayedBGM;

    [Header("BGM Settings")]
    [Tooltip("BGM을 재생하고 싶지 않은 씬의 이름을 정확하게 입력하세요.")]
    public List<string> scenesToMuteBGM = new List<string> { "Scene01", "MainScene" };

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
        {
            sfxDictionary[clip.name] = clip; 
        }

        bgmSource.Stop();
        bgmSource.clip = null;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    private void Start()
    {
        ResetAudioMixerAndApplyVolume();
    }
    private IEnumerator DetectAndSetCurrentBGM()
    {
        yield return null;

        Debug.Log("[AudioManager] 🕵️‍♂️ BGM 소스 탐색을 시작합니다...");
        bool foundBgm = false;

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source.outputAudioMixerGroup == bgmGroup && source.isPlaying)
            {
                lastPlayedBGM = source.clip;
                foundBgm = true;
                Debug.Log($"<color=lime>[AudioManager] ✨ 1순위 탐색 성공! 오브젝트: '{source.gameObject.name}', 클립: '{lastPlayedBGM.name}'.</color>");
                break;
            }
        }

        if (!foundBgm)
        {
            Debug.LogWarning("[AudioManager] 1순위 탐색(믹서 그룹) 실패. 2순위 탐색('BGM' 태그)을 시작합니다...");
            GameObject bgmObject = GameObject.FindGameObjectWithTag("BGM");

            if (bgmObject != null)
            {
                AudioSource sceneBgmSource = bgmObject.GetComponent<AudioSource>();
                if (sceneBgmSource != null && sceneBgmSource.clip != null)
                {
                    Debug.Log($"<color=cyan>[AudioManager] ✨ 2순위 탐색 성공! '{bgmObject.name}' 오브젝트에서 BGM을 찾아 재생합니다.</color>");
                    PlayBGM(sceneBgmSource.clip);
                    foundBgm = true; 
                }
            }
        }

        if (!foundBgm)
        {
            Debug.LogWarning("[AudioManager] 모든 방법으로 탐색했지만 현재 씬에서 BGM 소스를 찾지 못했습니다.");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scenesToMuteBGM.Contains(scene.name))
        {
            Debug.Log($"[AudioManager] '{scene.name}' 씬이므로 BGM 재생을 비활성화합니다.");
            canPlayBGM = false;
            StopBGM();
            return; 
        }
        else
        {
            canPlayBGM = true;
        }

        StartCoroutine(DetectAndSetCurrentBGM());
    }

    public void LoadVolumeSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.2f);
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.2f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.2f);

        Debug.Log($"[AudioManager] 시작 시 불러온 Master: {masterVolume}, BGM: {bgmVolume}, SFX: {sfxVolume}");


        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);

        Debug.Log("AudioManager가 시작 시 저장된 오디오 설정을 불러와 적용했습니다.");
    }
    public void ResetAudioMixerAndApplyVolume()
    {
        StartCoroutine(Coroutine_ResetAudioMixer());
    }

    private IEnumerator Coroutine_ResetAudioMixer()
    {
        Debug.Log("[AudioManager] 오디오 믹서 리셋을 시작합니다.");

        float targetBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.2f);

        audioMixer.SetFloat("BGMVolume", -80f);

        yield return null;

        SetBGMVolume(targetBgmVolume);

        Debug.Log($"[AudioManager] 오디오 믹서 리셋 완료. BGM 볼륨을 {targetBgmVolume}으로 복구했습니다.");
    }


    public void PlayBGM(AudioClip clip)

        Debug.Log($"[AudioManager] PlayBGM 호출됨. canPlayBGM 상태: {canPlayBGM}, clip 이름: {(clip != null ? clip.name : "null")}");

        if (!canPlayBGM || clip == null)
        {

            if (!canPlayBGM) Debug.LogWarning("[AudioManager] canPlayBGM이 false라서 BGM 재생을 차단했습니다.");
            return;
        }

        if (bgmSource.isPlaying && bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();

        lastPlayedBGM = clip;

        Debug.Log($"[AudioManager] {clip.name} BGM 재생을 시작합니다."); 
    }

    public void PlaySFX(string soundName)
    {
        if (sfxDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"효과음 '{soundName}'을 찾을 수 없습니다!");
        }
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log($"🔊 Master Volume 설정: {volume}");
        if (volume <= 0)
        {
            audioMixer.SetFloat("MasterVolume", -80f); 
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
    }


    public void SetBGMVolume(float volume)
    {
        if (volume <= 0) /
        {
            audioMixer.SetFloat("BGMVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }

    public AudioClip GetPillarClip(int index)
    {
        if (index >= 0 && index < pillarClips.Length)
            return pillarClips[index];

        Debug.LogWarning("Invalid pillar index for audio clip: " + index);
        return null;
    }
    public void PlayButtonClickSound()
    {
        if (buttonClickSFX == null)
        {
            Debug.LogWarning("🚨 버튼 클릭 효과음이 없습니다! AudioManager에서 설정하세요.");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("🚨 AudioManager: sfxSource가 null입니다! Unity에서 AudioSource를 추가하세요.");
            return;
        }

        sfxSource.PlayOneShot(buttonClickSFX);
    }
    public void PlaySFXByIndex(int index)
    {
        if (index >= 0 && index < sfxClips.Count)
        {
            sfxSource.PlayOneShot(sfxClips[index]);
        }
        else
        {
            Debug.LogWarning($"❌ [AudioManager] 유효하지 않은 SFX 인덱스: {index}");
        }
    }
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
    }

public void ResumeBGM()
{
    Debug.Log($"[AudioManager] BGM 재개를 시도합니다. 마지막 BGM: {(lastPlayedBGM != null ? lastPlayedBGM.name : "없음")}");

    if (canPlayBGM && lastPlayedBGM != null)
    {

        PlayBGM(lastPlayedBGM);
    }
    else
    {
        if (!canPlayBGM) Debug.Log("[AudioManager] canPlayBGM이 false라 BGM을 재개하지 않습니다.");
        if (lastPlayedBGM == null) Debug.Log("[AudioManager] 마지막으로 재생한 BGM에 대한 정보가 없어 재개하지 않습니다.");
    }
}
}
