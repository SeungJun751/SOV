using System.Collections.Generic;
using UnityEngine;

public class LoadGamePanel : MonoBehaviour
{
    [Header("설정")]
    public GameObject saveSlotPrefab;     
    public Transform slotContainer;        

    [Header("로드할 데이터 목록")]
    public List<SaveSlotData> saveSlots;

    void Start()
    {
        GenerateSlots();
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var slotData in saveSlots)
        {
            GameObject slotInstance = Instantiate(saveSlotPrefab, slotContainer);
            slotInstance.GetComponent<SaveSlotUI>().Setup(slotData);
        }
    }
}