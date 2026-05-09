// 🐾 AnimalSoundTrailPuzzle.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSoundTrailPuzzle : MonoBehaviour
{
    [Header("🔊 동물 소리 이동 관련")]
    public AudioClip animalSoundClip;
    public GameObject movingAudioPrefab;
    public float moveDuration = 2.0f;

    [Header("🌳 나무 위치")]
    public Transform[] treePositions = new Transform[5];

    private List<int> soundPattern = new List<int>();
    public Coroutine trailRoutine = null;
    private List<GameObject> activeAudioObjects = new();

    public void StartPuzzleFrom(Vector3 startPosition)
    {
        foreach (var obj in activeAudioObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        activeAudioObjects.Clear();

        if (this.trailRoutine != null)
        {
            StopCoroutine(this.trailRoutine);
            this.trailRoutine = null;
        }

        soundPattern.Clear();
        int startIndex = FindClosestTreeIndex(startPosition);
        soundPattern.Add(startIndex);

        List<int> available = new List<int> { 0, 1, 2, 3, 4 };
        available.Remove(startIndex);

        while (soundPattern.Count < 5 && available.Count > 0)
        {
            int next = available[Random.Range(0, available.Count)];
            soundPattern.Add(next);
            available.Remove(next);
        }

        this.trailRoutine = StartCoroutine(PlayAnimalTrail());
    }

    private IEnumerator PlayMovingSound(Vector3 startPos, Vector3 endPos)
    {
        GameObject movingAudioObj = Instantiate(movingAudioPrefab, startPos, Quaternion.identity);
        activeAudioObjects.Add(movingAudioObj);

        AudioSource audioSource = movingAudioObj.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = movingAudioObj.AddComponent<AudioSource>();

        audioSource.clip = animalSoundClip;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 30f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            if (movingAudioObj == null) yield break; 

            movingAudioObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (movingAudioObj != null)
            movingAudioObj.transform.position = endPos;

        if (movingAudioObj != null)
            Destroy(movingAudioObj, animalSoundClip.length + 0.5f);
    }



    private int FindClosestTreeIndex(Vector3 pos)
    {
        float closestDist = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < treePositions.Length; i++)
        {
            float dist = Vector3.Distance(pos, treePositions[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private IEnumerator PlayAnimalTrail()
    {
        for (int i = 0; i < soundPattern.Count - 1; i++)
        {
            int from = soundPattern[i];
            int to = soundPattern[i + 1];

            yield return StartCoroutine(PlayMovingSound(
                treePositions[from].position,
                treePositions[to].position
            ));
            yield return new WaitForSeconds(0.5f);
        }

        this.trailRoutine = null; 
    }

    public List<int> GetSoundPattern()
    {
        return new List<int>(soundPattern);
    }
}
