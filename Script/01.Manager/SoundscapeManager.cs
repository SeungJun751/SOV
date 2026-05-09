using UnityEngine;
using System.Collections.Generic;

public class SoundscapeManager : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;

    [Header("Sound Source Settings")]
    public AudioClip[] clipsToScatter;
    public int numberOfSounds = 100;
    public Vector3 areaSize = new Vector3(100, 0, 100);

    [Header("Sound Properties")]
    [Range(0f, 1f)]
    public float minVolume = 0.8f;
    public float maxVolume = 1.0f;
    public float maxDistance = 40f;

    [Header("Dynamic Update Settings")]
    public float respawnDistance = 120f;
    public float updateInterval = 5f;
    private float updateTimer;

    [Header("Random Playback Settings")]
    public float minInitialDelay = 0f;
    public float maxInitialDelay = 10f;
    public float minRepeatDelay = 5f;
    public float maxRepeatDelay = 15f;

    private List<VirtualSound> managedSounds = new List<VirtualSound>();

    void Start()
    {
        if (SpatialAudioManager.Instance == null || playerTransform == null)
        {
            Debug.LogError("SpatialAudioManager 또는 Player Transform이 할당되지 않았습니다!");
            this.enabled = false;
            return;
        }

        for (int i = 0; i < numberOfSounds; i++)
        {
            CreateSoundNearPlayer();
        }
    }

    void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            UpdateSoundPositions();
        }
    }

    private void CreateSoundNearPlayer()
    {
        Vector3 center = playerTransform.position;
        float x = Random.Range(center.x - areaSize.x / 2, center.x + areaSize.x / 2);
        float y = center.y;
        float z = Random.Range(center.z - areaSize.z / 2, center.z + areaSize.z / 2);
        Vector3 randomPosition = new Vector3(x, y, z);
        AudioClip randomClip = clipsToScatter[Random.Range(0, clipsToScatter.Length)];

        VirtualSound newSound = new VirtualSound
        {
            position = randomPosition,
            clip = randomClip,
            volume = Random.Range(minVolume, maxVolume),
            maxDistance = this.maxDistance,
            nextPlayTime = Time.time + Random.Range(minInitialDelay, maxInitialDelay),
            minRepeatDelay = this.minRepeatDelay,
            maxRepeatDelay = this.maxRepeatDelay
        };

        managedSounds.Add(newSound);
        SpatialAudioManager.Instance.RegisterVirtualSound(newSound);
    }

    private void UpdateSoundPositions()
    {
        foreach (VirtualSound sound in managedSounds)
        {
            float distanceToPlayer = Vector3.Distance(sound.position, playerTransform.position);

            if (distanceToPlayer > respawnDistance)
            {
                Vector3 center = playerTransform.position;
                float x = Random.Range(center.x - areaSize.x / 2, center.x + areaSize.x / 2);
                float y = center.y;
                float z = Random.Range(center.z - areaSize.z / 2, center.z + areaSize.z / 2);
                sound.position = new Vector3(x, y, z);
            }
        }
    }
}