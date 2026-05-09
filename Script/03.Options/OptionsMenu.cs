using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button graphicsTabButton;
    public Button soundTabButton;
    public Button controlsTabButton;

    [Header("콘텐츠 패널")]
    public GameObject graphicsPanel;
    public GameObject soundPanel;
    public GameObject controlsPanel;

    [Header("그래픽 설정 UI")]
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    public Toggle vSyncToggle;
    public Button applyButton;

    [Header("사운드 설정 UI")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Dropdown speakerModeDropdown;

    [Header("컨트롤 설정 UI")]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityValueText;

    [Header("기타 UI")]
    public Button goToMainMenuButton; 

    [Header("Scene Management")] 
    [Tooltip("돌아갈 메인 메뉴 씬의 이름을 정확하게 입력하세요.")]
    public string mainMenuSceneName = "MainScene"; 

    private Resolution[] resolutions;

    void Start()
    {
        SetupResolutions();
        SetupQuality();
        SetupSpeakerModes();
        AddListeners();
        SwitchTab("Graphics");
    }
    private void OnEnable()
    {
        StartCoroutine(LoadSettingsAfterFrame());
    }
    private IEnumerator LoadSettingsAfterFrame()
    {
        yield return null;
        LoadSettings();
    }

    private void AddListeners()
    {
        graphicsTabButton.onClick.AddListener(() => SwitchTab("Graphics"));
        soundTabButton.onClick.AddListener(() => SwitchTab("Sound"));
        controlsTabButton.onClick.AddListener(() => SwitchTab("Controls"));
        applyButton.onClick.AddListener(ApplyAndSaveGraphics);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);

        goToMainMenuButton.onClick.AddListener(GoToMainMenu);

        speakerModeDropdown.onValueChanged.AddListener(SetSpeakerMode);
    }

    public void GoToMainMenu()
    {
        UIManager.Instance.CloseOptionsMenu();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 탭 전환 로직
    public void SwitchTab(string tabName)
    {
        graphicsPanel.SetActive(tabName == "Graphics");
        soundPanel.SetActive(tabName == "Sound");
        controlsPanel.SetActive(tabName == "Controls");

        // (UX 개선) 현재 활성화된 탭 버튼 색상 변경
        graphicsTabButton.interactable = (tabName != "Graphics");
        soundTabButton.interactable = (tabName != "Sound");
        controlsTabButton.interactable = (tabName != "Controls");
    }

    #region 설정 불러오기 (Load)
    private void LoadSettings()
    {
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        qualityDropdown.value = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;

        resolutionDropdown.RefreshShownValue();
        qualityDropdown.RefreshShownValue();
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.2f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.2f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.2f);

        speakerModeDropdown.value = PlayerPrefs.GetInt("SpeakerMode", 0);
        speakerModeDropdown.RefreshShownValue();

        mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
        Debug.Log("저장된 오디오 설정을 불러와 믹서에 적용 시도.");
    }
    #endregion

    #region 사운드 설정 (적용 및 저장)
    private void SetMasterVolume(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void SetBGMVolume(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    public void SetSpeakerMode(int index)
    {
        Debug.Log($"SetSpeakerMode 함수 호출됨. 인덱스: {index}");

        AudioSpeakerMode speakerMode;
        switch (index)
        {
            case 1:
                speakerMode = AudioSpeakerMode.Mode5point1;
                break;
            case 2:
                speakerMode = AudioSpeakerMode.Mode7point1;
                break;
            default:
                speakerMode = AudioSpeakerMode.Stereo;
                break;
        }
        var previousMode = AudioSettings.speakerMode;

        AudioSettings.speakerMode = speakerMode;

        var currentMode = AudioSettings.speakerMode;

        PlayerPrefs.SetInt("SpeakerMode", index);
        PlayerPrefs.Save();

        Debug.Log($"스피커 모드 변경 시도: '{previousMode}' -> '{currentMode}'. 저장된 인덱스: {index}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeBGM();
        }

        if (previousMode == currentMode && speakerMode != previousMode)
        {
            Debug.LogError("경고: AudioSettings.speakerMode가 변경되지 않았습니다! 다른 설정이나 하드웨어 제한을 확인하세요.");
        }
    }
    #endregion

    #region 그래픽 설정 (적용 및 저장)
    private void ApplyAndSaveGraphics()
    {
        Resolution res = resolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;

        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("QualityIndex", qualityDropdown.value);
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);

        PlayerPrefs.Save(); 
        Debug.Log("그래픽 설정이 적용 및 저장되었습니다.");
    }
    #endregion

    #region UI 초기화 헬퍼 함수
    private void SetupResolutions()
    {
        var commonResolutions = new HashSet<Vector2Int>
    {
        new Vector2Int(1280, 720), 
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440), 
        new Vector2Int(3840, 2160), 
    };
        resolutions = Screen.resolutions
            .Where(res => commonResolutions.Contains(new Vector2Int(res.width, res.height)))
            .Select(res => new Resolution { width = res.width, height = res.height })
            .Distinct()
            .ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
        }
        resolutionDropdown.AddOptions(options);
    }
    private void SetupQuality()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }
    private void SetupSpeakerModes()
    {
        speakerModeDropdown.ClearOptions();
        speakerModeDropdown.AddOptions(new List<string> { "Stereo", "Surround 5.1", "Surround 7.1" });
    }
    #endregion
    #region 컨트롤 설정 (적용 및 저장)
    public void SetMouseSensitivity(float value)
    {
        if (mouseSensitivityValueText != null)
        {
            mouseSensitivityValueText.text = value.ToString("F2");
        }

        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        Debug.Log("감도 설정 및 저장됨: " + value);
        if (CameraController.Instance != null)
        {
            CameraController.Instance.UpdateSensitivity(value);
        }
    }
    #endregion
    public void OnDropdownUsed()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}