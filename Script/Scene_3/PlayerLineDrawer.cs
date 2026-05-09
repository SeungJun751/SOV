using System.Collections.Generic;
using UnityEngine;
using DoorScript;

public class PlayerLineDrawer : MonoBehaviour
{
    public static bool AnimalPuzzleCleared = false;

    [Header("🚪 퍼즐 성공 시 상호작용")]
    public Door door;

    public GameObject linePrefab;  
    public Transform playerCamera; 
    private List<LineRenderer> drawnLines = new(); 
    private LineRenderer activeLine;                
    private bool isTracking = false;
    private Vector3 yOffset = new Vector3(0, 0.8f, 0); 
    private HashSet<Transform> visitedTrees = new();   
    private Transform lastTree = null;               

    public int maxTreeCount = 5;  
    private bool puzzleComplete = false;  

    public Transform[] allTrees; 
    private List<int> playerPath = new();
    public AnimalSoundTrailPuzzle animalPuzzle; 
    private int firstCorrectTreeIndex = -1;
    private int puzzleAttempts = 0; 
    public AnimalPuzzleStarter animalPuzzleStarter; 
    public int maxAttemptsBeforeRetrySound = 5;

    void Awake() 
    {
        AnimalPuzzleCleared = false;
    }

    void Start()
    {
        if (playerCamera == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                Camera foundCam = playerObj.GetComponentInChildren<Camera>();
                if (foundCam != null)
                {
                    playerCamera = foundCam.transform;
                }
                else
                {
                    Debug.LogWarning("플레이어 오브젝트에 카메라가 없습니다.");
                }
            }
            else if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
            else
            {
                Debug.LogError("플레이어 카메라를 자동으로 찾을 수 없습니다!");
            }
        }
        if (animalPuzzleStarter == null)
        {
            animalPuzzleStarter = FindAnyObjectByType<AnimalPuzzleStarter>();
            if (animalPuzzleStarter != null)
            {
                Debug.Log("[PlayerLineDrawer] AnimalPuzzleStarter 자동 연결 완료.");
            }
            else
            {
                Debug.LogWarning("[PlayerLineDrawer] AnimalPuzzleStarter를 씬에서 찾을 수 없습니다. 수동 할당이 필요할 수 있습니다.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryConnectToTree();
        }

        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            UndoLastConnection();
        }


        if (isTracking && activeLine != null)
        {
            activeLine.SetPosition(1, GetLookingTreePoint());
            if (lastTree == null)
            {
                activeLine.SetPosition(0, playerCamera.position + yOffset);
            }
            else
            {
                activeLine.SetPosition(0, lastTree.position + yOffset);
            }
        }
    }
    public void UndoLastConnection()
    {
        if (puzzleComplete) return;

        if (playerPath.Count == 0 || visitedTrees.Count == 0)
        {
            if (activeLine != null)
            {
                Destroy(activeLine.gameObject);
                activeLine = null;
            }

            lastTree = null;
            isTracking = false;
            Debug.Log("⏪ 모든 연결이 취소되어 초기 상태입니다.");
            return;
        }

        if (lastTree != null)
        {
            visitedTrees.Remove(lastTree);

            if (drawnLines.Count > 0)
            {
                var lastLine = drawnLines[drawnLines.Count - 1];
                Destroy(lastLine.gameObject);
                drawnLines.RemoveAt(drawnLines.Count - 1);
            }

            if (playerPath.Count > 0)
                playerPath.RemoveAt(playerPath.Count - 1);

            Transform[] treeArray = new Transform[visitedTrees.Count];
            visitedTrees.CopyTo(treeArray);
            lastTree = treeArray.Length > 0 ? treeArray[treeArray.Length - 1] : null;

            if (activeLine != null)
                Destroy(activeLine.gameObject);

            if (lastTree != null)
            {
                GameObject lineObj = Instantiate(linePrefab);
                activeLine = lineObj.GetComponent<LineRenderer>();
                activeLine.positionCount = 2;
                activeLine.SetPosition(0, lastTree.position + yOffset);
                activeLine.SetPosition(1, GetLookingTreePoint());
                isTracking = true;
            }
            else
            {
                activeLine = null;
                isTracking = false;
            }

            isTracking = true;
        }
    }



    void TryConnectToTree()
    {
        if (puzzleComplete) return; 

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                Transform newTree = hit.collider.transform;

                if (visitedTrees.Contains(newTree))
                {
                    Debug.Log("이미 선택된 나무입니다.");
                    return;
                }
                if (playerPath.Count == 0)
                {
                    int clickedIndex = GetTreeIndex(newTree);
                    if (clickedIndex != firstCorrectTreeIndex)
                    {
                        return;
                    }
                }

                ConnectToTree(newTree);
                if (playerPath.Count == maxTreeCount)
                {
                    var correctPath = animalPuzzle.GetSoundPattern();
                    Debug.Log("🐾 동물 경로: " + string.Join(",", correctPath));
                    Debug.Log("🧍 플레이어 경로: " + string.Join(",", playerPath));

                    if (IsSameAs(correctPath))
                    {
                        CompletePuzzle();
                    }
                    puzzleAttempts++; 
                    if (puzzleAttempts >= maxAttemptsBeforeRetrySound) .
                    {
                        Debug.Log($"[P
                        if (animalPuzzleStarter != null)
                        {
                            animalPuzzleStarter.RetryPuzzleSequence(); 
                        }
                        puzzleAttempts = 0; 
                    }
                    else 
                    {
                        ResetLines(); 
                        Debug.Log($"남은 시도 횟수: {maxAttemptsBeforeRetrySound - puzzleAttempts}");
                    }
                }
            }
        }
    }


    void ConnectToTree(Transform tree)
    {
        if (tree == null || linePrefab == null || playerCamera == null)
            return;

        visitedTrees.Add(tree);
        playerPath.Add(GetTreeIndex(tree));
        Vector3 endPos = tree.position + yOffset;

        if (lastTree == null)
        {
            GameObject lineObj = Instantiate(linePrefab);
            activeLine = lineObj.GetComponent<LineRenderer>();
            activeLine.material = new Material(activeLine.material);
            activeLine.positionCount = 2;
            activeLine.SetPosition(0, playerCamera.position); 
            activeLine.SetPosition(1, endPos);
            isTracking = true;
        }
        else
        {
            GameObject lineObj = Instantiate(linePrefab);
            LineRenderer fixedLine = lineObj.GetComponent<LineRenderer>();
            fixedLine.material = new Material(fixedLine.material);
            fixedLine.positionCount = 2;
            fixedLine.SetPosition(0, lastTree.position + yOffset);
            fixedLine.SetPosition(1, endPos);
            drawnLines.Add(fixedLine);

            if (activeLine != null)
            {
                Destroy(activeLine.gameObject);
                activeLine = null;
            }

            if (visitedTrees.Count >= maxTreeCount)
            {
                isTracking = false;
                lastTree = tree;
                return;
            }

            activeLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
            activeLine.material = new Material(activeLine.material);
            activeLine.positionCount = 2;
            activeLine.SetPosition(0, tree.position + yOffset);
            activeLine.SetPosition(1, tree.position + yOffset);
            isTracking = true;
        }
        lastTree = tree;
    }

    private Vector3 GetLookingTreePoint()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                return hit.collider.transform.position + yOffset;
            }
        }

        return playerCamera.position + playerCamera.forward * 2f + yOffset;
    }

    int GetTreeIndex(Transform tree)
    {
        for (int i = 0; i < allTrees.Length; i++)
        {
            if (allTrees[i] == tree)
                return i;
        }
        return -1;
    }


    public void ResetLines()
    {
        if (puzzleComplete) return; 

        foreach (var line in drawnLines)
            Destroy(line.gameObject);

        if (activeLine != null)
            Destroy(activeLine.gameObject);

        drawnLines.Clear();
        visitedTrees.Clear();
        playerPath.Clear();
        activeLine = null;
        lastTree = null;
        isTracking = false;
        puzzleComplete = false;
    }

    private void CompletePuzzle()
    {
        puzzleComplete = true;
        isTracking = false;

        if (activeLine != null)
        {
            Destroy(activeLine.gameObject);
            activeLine = null;
        }

        foreach (var line in drawnLines)
        {
            if (line != null && line.material != null)
            {
                line.material.color = Color.yellow;
            }
        }
        puzzleAttempts = 0;
        AnimalPuzzleCleared = true;

        if (door != null)
        {
            door.OpenDoor();
        }
        else
        {
            Debug.LogWarning("'Door'가 연결되지 않아 문을 열 수 없습니다.");
        }
    }

    private bool IsSameAs(List<int> targetPattern)
    {
        if (playerPath.Count != targetPattern.Count)
            return false;

        for (int i = 0; i < playerPath.Count; i++)
        {
            if (playerPath[i] != targetPattern[i])
                return false;
        }

        return true;
    }
    public void SetCorrectPath(List<int> path)
    {
        if (path != null && path.Count > 0)
            firstCorrectTreeIndex = path[0];
    }
}