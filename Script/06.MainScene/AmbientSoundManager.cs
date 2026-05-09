using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientSoundManager : MonoBehaviour
{
    public AudioClip[] ambientSounds; 
    public Transform caveCenter; 
    public float radius = 10f; 
    public float minInterval = 5f; 
    public float maxPlayTime = 10f; 
    public AudioSource ambientAudioSource; 

    private List<AudioSource> activeSounds = new List<AudioSource>(); 

    void Start()
    {
        if (ambientAudioSource == null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>(); 
            ambientAudioSource.spatialBlend = 1f; 
            ambientAudioSource.loop = false;
        }

        StartCoroutine(PlayRandomAmbientSound());
    }

    IEnumerator PlayRandomAmbientSound()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            PlayAmbientSound();
        }
    }

    void PlayAmbientSound()
    {
        if (ambientSounds == null || ambientSounds.Length == 0) return;
        int randomIndex = Random.Range(0, ambientSounds.Length);
        AudioClip randomClip = ambientSounds[randomIndex];

        Debug.Log($"🎶 재생할 랜덤 사운드: {randomClip.name} (Index: {randomIndex})");
        Vector3 randomOffset = new Vector3(
            Random.Range(-radius, radius), 
            Random.Range(-radius * 0.5f, radius * 0.5f), 
            Random.Range(-radius, radius) 
        );

        Vector3 spawnPosition = caveCenter.position + randomOffset;

        AudioSource soundSource = gameObject.AddComponent<AudioSource>();
        soundSource.clip = randomClip;
        soundSource.spatialBlend = 1f; 
        soundSource.rolloffMode = AudioRolloffMode.Custom;
        soundSource.minDistance = 10f; 
        soundSource.maxDistance = radius; 
        soundSource.spatialize = true;
        soundSource.volume = ambientAudioSource.volume;
        soundSource.Play();

        activeSounds.Add(soundSource);

        Debug.Log($"🎶 [Ambient Sound] {randomClip.name} at {spawnPosition}");
        Debug.DrawLine(caveCenter.position, spawnPosition, Color.red, 10f);  
        StartCoroutine(FadeOutAndDestroy(soundSource));
    }


    IEnumerator FadeOutAndDestroy(AudioSource audioSource)
    {
        float fadeDuration = 2f; 
        float elapsedTime = 0f;

        while (elapsedTime < maxPlayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        float startVolume = audioSource.volume;
        elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        activeSounds.Remove(audioSource);
        Destroy(audioSource);
    }

    public void SetAmbientVolume(float volume)
    {
        ambientAudioSource.volume = volume;
        foreach (AudioSource source in activeSounds)
        {
            source.volume = volume; 
        }
    }
}
