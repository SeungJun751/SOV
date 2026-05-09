using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] 
public class CharacterSelectButton : MonoBehaviour
{
    public int characterIndex;

    void Start()
    {
        Button button = GetComponent<Button>();

        if (CharacterSelectionManager.Instance != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                CharacterSelectionManager.Instance.SelectCharacter(characterIndex);
            });
        }
        else
        {
            Debug.LogError("CharacterSelectionManager 인스턴스를 찾을 수 없습니다!");
        }
    }
}