using UnityEngine;

public class WaterSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] soundClips;

    private float baseHeight;
    private float range = 2f; 
    private int totalNotes = 8;

    public int expectedNote = -1;
    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 30f;
        baseHeight = transform.position.y;
    }

    public int GetCurrentNoteIndex()
    {
        float currentRange = (transform.position.y - baseHeight) + (range / 2f);
        float normalized = Mathf.Clamp01(currentRange / range);
        return Mathf.RoundToInt(normalized * (totalNotes - 1));
    }

    public void ResetHeight()
    {
        var pos = transform.position;
        pos.y = baseHeight;
        transform.position = pos;
    }

    public void PlayNoteSound(int index)
    {
        if (index < 0 || index >= soundClips.Length) return;

        audioSource.clip = soundClips[index];
        audioSource.Play();
    }
}