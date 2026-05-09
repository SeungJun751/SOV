using UnityEngine;

[CreateAssetMenu(fileName = "NewSaveSlot", menuName = "Game/Save Slot Data")]
public class SaveSlotData : ScriptableObject
{
    [Header("표시될 정보")]
    public string sceneDisplayName; 
    public string characterName;      
    public Sprite characterIcon;      

    [Header("내부 데이터")]
    public string targetSceneName;    
    public int characterIndex;       
}