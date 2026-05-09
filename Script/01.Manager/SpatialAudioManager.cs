using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

public class SpatialAudioManager : MonoBehaviour
{
    public static SpatialAudioManager Instance;

    [Header("Audio Pool Settings")]
    public int poolSize = 32;
    private List<AudioSource> audioSourcePool;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Listener")]
    public Transform listenerTransform;

    private List<VirtualSound> virtualSounds = new List<VirtualSound>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject sourceObj = new GameObject("PooledAudioSource_" + i);
            sourceObj.transform.SetParent(this.transform);
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.spatialBlend = 1.0f;
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.gameObject.SetActive(false);
            audioSourcePool.Add(source);
        }
    }

    void Update()
    {
        if (listenerTransform == null) return;

        foreach (var source in audioSourcePool)
        {
            if (source.gameObject.activeSelf && !source.isPlaying)
            {
                source.gameObject.SetActive(false);
            }
        }
        var sortedSounds = virtualSounds.OrderBy(
            sound => Vector3.Distance(sound.position, listenerTransform.position)
        ).ToList();

        foreach (VirtualSound vs in sortedSounds)
        {
            if (Time.time >= vs.nextPlayTime && Vector3.Distance(vs.position, listenerTransform.position) <= vs.maxDistance)
            {
                AudioSource availableSource = audioSourcePool.FirstOrDefault(s => !s.gameObject.activeSelf);

                if (availableSource != null)
                {
                    availableSource.transform.position = vs.position;
                    availableSource.clip = vs.clip;
                    availableSource.volume = vs.volume;
                    availableSource.maxDistance = vs.maxDistance;
                    availableSource.loop = false; 
                    availableSource.gameObject.SetActive(true);
                    availableSource.Play();

                    vs.nextPlayTime = Time.time + vs.clip.length + Random.Range(vs.minRepeatDelay, vs.maxRepeatDelay);
                }
            }
        }
    }

    public void RegisterVirtualSound(VirtualSound sound)
    {
        virtualSounds.Add(sound);
    }
}