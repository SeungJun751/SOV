using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SceneBGM
{
    public string sceneName; 
    public AudioClip bgmClip;
}

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;         
    public GameObject characterPrefab; 
    public List<SceneBGM> sceneBGMs;     
}