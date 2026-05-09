using UnityEngine;

[System.Serializable]
public class VirtualSound
{
    public Vector3 position;
    public AudioClip clip;
    public float volume = 1f;
    public float maxDistance = 50f;

    public float nextPlayTime;

    public float minRepeatDelay = 5f;
    public float maxRepeatDelay = 15f;
}