using UnityEngine;
using UnityEngine.EventSystems;

public class NoteCube : MonoBehaviour
{
    [Header("오디오")]
    public AudioSource audioSource;
    public AudioClip[] noteClips;

    [Header("설정")]
    public float loopInterval = 0.8f;   

    [Header("연동")]
    public PlayerControlBase playerControlBase;
    public Transform player;          

    [Header("바다 퍼즐용")]
    public int expectedNote = -1;  

    private int assignedNoteIndex = -1;
    public int AssignedNoteIndex => assignedNoteIndex;
    private float nextPlayTime = 0f;
    private bool isLooping = false;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (playerControlBase == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerControlBase = playerObj.GetComponent<PlayerControlBase>();
                player = playerObj.transform;
            }
        }

        if (assignedNoteIndex < 0)
            AssignRandomNote();
    }

    void Update()
    {
        if (player != null && Input.GetKeyDown(KeyCode.F))
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= 4f)
            {
                PlayAssignedNoteOnce();
            }
        }

        if (isLooping && Time.time >= nextPlayTime)
        {
            PlayAssignedNoteOnce();
            nextPlayTime = Time.time + loopInterval;
        }
    }

    public void InitializeNoteCube()
    {
        assignedNoteIndex = Random.Range(0, 8);   
        if (assignedNoteIndex < 0)
            AssignRandomNote();

        if (playerControlBase == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerControlBase = playerObj.GetComponent<PlayerControlBase>();
        }

        expectedNote = assignedNoteIndex;
    }

    public void AssignRandomNote()
    {
        if (noteClips == null || noteClips.Length == 0)
        {
            Debug.LogWarning($"{name}: noteClips가 비어있습니다.");
            assignedNoteIndex = -1;
            return;
        }
        assignedNoteIndex = Random.Range(0, Mathf.Min(8, noteClips.Length));
    }

    public void SetAssignedNote(int noteIndex)
    {
        assignedNoteIndex = noteIndex;
    }

    private void PlayAssignedNoteOnce()
    {
        if (assignedNoteIndex < 0 || noteClips == null || assignedNoteIndex >= noteClips.Length)
            return;
        if (audioSource == null)
            return;

        audioSource.clip = noteClips[assignedNoteIndex];
        audioSource.Play();
    }

    public void ToggleLoop()
    {
        isLooping = !isLooping;
        if (isLooping)
        {
            PlayAssignedNoteOnce();
            nextPlayTime = Time.time + loopInterval;
        }
        else
        {
            StopPlaying();
        }
    }

    public void StopPlaying()
    {
        isLooping = false;
        if (audioSource != null)
            audioSource.Stop();
    }
}


