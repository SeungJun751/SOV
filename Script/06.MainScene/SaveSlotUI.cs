using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public Image characterIcon;
    public Text sceneNameText;
    public Text characterNameText;
    public Button loadButton;

    private string sceneToLoad;
    private int charToSelect;
    public void Setup(SaveSlotData data)
    {
        characterIcon.sprite = data.characterIcon;
        sceneNameText.text = data.sceneDisplayName;
        characterNameText.text = data.characterName;

        sceneToLoad = data.targetSceneName;
        charToSelect = data.characterIndex;

        loadButton.onClick.AddListener(LoadThisSlot);
    }

    private void LoadThisSlot()
    {
        Debug.Log($"로드 요청: Scene '{sceneToLoad}', Character Index '{charToSelect}'");

        var characterManager = CharacterSelectionManager.Instance;
        if (characterManager != null)
        {
            characterManager.SelectCharacter(charToSelect);
        }

        LoadingManager loadingManager = FindFirstObjectByType<LoadingManager>();
        if (loadingManager != null)
        {
            loadingManager.LoadScene(sceneToLoad);
        }
        else 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}