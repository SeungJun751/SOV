using UnityEngine;

public class SceneUIConnector : MonoBehaviour
{
    [Header("연결할 UI 오브젝트")]
    public GameObject characterSelectionPanel;


    void Awake()
    {
        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.characterSelectionPanel = this.characterSelectionPanel;

            Debug.Log(characterSelectionPanel.name + "의 참조가 CharacterSelectionManager에 성공적으로 연결되었습니다.");
        }
        else
        {
            Debug.LogError("CharacterSelectionManager 인스턴스를 찾을 수 없습니다. 씬에 해당 매니저가 존재하는지 확인하세요.");
        }
    }
}