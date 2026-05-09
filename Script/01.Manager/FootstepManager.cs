using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TerrainSound
{
    public string name; 
    public Texture2D terrainTexture; 
    public AudioClip[] footstepSounds; 
}

public class FootstepManager : MonoBehaviour
{
    [Tooltip("소리를 재생할 오디오 소스")]
    public AudioSource audioSource;

    [Tooltip("플레이어가 밟고 있는 터레인")]
    public Terrain terrain;

    [Tooltip("텍스처와 사운드 매칭 리스트")]
    public List<TerrainSound> terrainSounds;


    private TerrainData terrainData;
    private Vector3 terrainPos;
    private float[,,] splatmapData;
    private int alphaMapWidth;
    private int alphaMapHeight;


    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain이 할당되지 않았습니다!");
            return;
        }

        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;
        alphaMapWidth = terrainData.alphamapWidth;
        alphaMapHeight = terrainData.alphamapHeight;
    }
    public void PlayFootstepSound()
    {
        if (!this.enabled || terrain == null || audioSource == null) return;

        int mainTextureIndex = GetMainTextureIndex(transform.position);

        Texture2D dominantTexture = terrainData.terrainLayers[mainTextureIndex].diffuseTexture;
        TerrainSound soundData = terrainSounds.FirstOrDefault(s => s.terrainTexture == dominantTexture);

        if (soundData != null && soundData.footstepSounds.Length > 0)
        {
            AudioClip clip = soundData.footstepSounds[Random.Range(0, soundData.footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }


    private int GetMainTextureIndex(Vector3 worldPos)
    {
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * alphaMapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * alphaMapHeight);

        splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float maxMix = 0;
        int maxIndex = 0;

        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {
            if (splatmapData[0, 0, i] > maxMix)
            {
                maxIndex = i;
                maxMix = splatmapData[0, 0, i];
            }
        }
        return maxIndex;
    }
}