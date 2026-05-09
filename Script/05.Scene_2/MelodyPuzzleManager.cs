using System.Collections;
using System.Collections.Generic;
using DoorScript;
using UnityEngine;

public class MelodyPuzzleManager : MonoBehaviour
{
    [Header("퍼즐 필수 구성 요소")]
    [Tooltip("플레이어가 높이를 조절할 물 구덩이들")]
    public WaterSound[] waterPits;

    [Tooltip("씬에 미리 배치한 노트 큐브들을 순서대로 할당해주세요.")]
    public NoteCube[] prePlacedCubes; 

    [Tooltip("퍼즐 클리어 시 열릴 문")]
    public Door puzzleDoor;

    public static bool PuzzleCleared { get; private set; } = false;

    void Start()
    {
        if (prePlacedCubes == null || waterPits == null || prePlacedCubes.Length != waterPits.Length)
        {
            Debug.LogError("MelodyPuzzleManager: 할당된 큐브와 물 구덩이의 개수가 다릅니다! 인스펙터에서 개수를 동일하게 맞춰주세요.");
            return;
        }

        InitializePuzzleNotes();
    }
    private void InitializePuzzleNotes()
    {
        for (int i = 0; i < prePlacedCubes.Length; i++)
        {
            NoteCube cube = prePlacedCubes[i];
            WaterSound pit = waterPits[i];

            if (cube == null || pit == null)
            {
                Debug.LogWarning($"[{i}]번째 큐브 또는 물 구덩이가 할당되지 않았습니다. 건너뜁니다.");
                continue;
            }

            cube.InitializeNoteCube();
            pit.expectedNote = cube.AssignedNoteIndex;
            pit.ResetHeight();
        }
    }

    public void CheckAnswerExternally()
    {
        if (waterPits == null || waterPits.Length == 0)
        {
            Debug.LogWarning("물 웅덩이가 할당되지 않아 정답을 확인할 수 없습니다.");
            return;
        }

        bool allCorrect = true;
        string[] noteNames = { "도", "레", "미", "파", "솔", "라", "시", "도(높은)" };

        for (int i = 0; i < waterPits.Length; i++)
        {
            var pit = waterPits[i];
            int actual = pit.GetCurrentNoteIndex();
            int expected = pit.expectedNote;

            if (expected != actual)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            if (puzzleDoor != null && !PuzzleCleared)
            {
                puzzleDoor.OpenDoor();
                PuzzleCleared = true;
                Debug.Log("퍼즐 성공! 문이 열립니다.");
            }
        }
        else
        {
            Debug.Log("오답 있음. 큐브 소리를 들으며 물 높이를 맞춰보세요.");
        }
    }

    [ContextMenu("Reset Puzzle (Rerandomize Notes)")]
    public void ResetPuzzle()
    {
        InitializePuzzleNotes(); /
        PuzzleCleared = false;
        Debug.Log("퍼즐이 리셋되었습니다. 큐브의 음계가 새로 할당되었습니다.");
    }
}