using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealingZoneManager : MonoBehaviour
{
    [Header("Prefab 세팅")]
    public GameObject windPrefab;
    public GameObject birdPrefab;

    [Header("플레이어 참조")]
    public Transform playerTransform;

    [Header("스폰 설정")]
    public float spawnRadius = 20f;
    public float windSpawnInterval = 5f;
    public float birdSpawnInterval = 30f;

    [Header("새 이동 설정")]
    public float birdSpeed = 5f;
    public float birdLifetime = 15f;
    public float birdHeightOffset = 7f; 

    private List<GameObject> activeBirds = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnWindRoutine());
        StartCoroutine(SpawnBirdRoutine());
    }

    private void Update()
    {
        MoveBirds();
    }

    private IEnumerator SpawnWindRoutine()
    {
        while (true)
        {
            SpawnWind();
            yield return new WaitForSeconds(windSpawnInterval);
        }
    }

    private IEnumerator SpawnBirdRoutine()
    {
        while (true)
        {
            SpawnBirds();
            yield return new WaitForSeconds(birdSpawnInterval);
        }
    }

    private void SpawnWind()
    {
        Vector3 randomPos = playerTransform.position + Random.onUnitSphere * spawnRadius;
        randomPos.y = Mathf.Clamp(randomPos.y, playerTransform.position.y, playerTransform.position.y + 5f);

        GameObject wind = Instantiate(windPrefab, randomPos, Quaternion.identity);
        Destroy(wind, 8f);
    }

    private void SpawnBirds()
    {
        int birdCount = Random.Range(1, 4); 

        Vector3 randomHorizontal = Random.onUnitSphere;
        randomHorizontal.y = 0f; 
        randomHorizontal.Normalize();

        Vector3 baseStartPos = playerTransform.position + randomHorizontal * (spawnRadius + 10f);

        float heightOffset = Random.Range(5f, 10f);
        baseStartPos.y = playerTransform.position.y + heightOffset;

        Vector3 toPlayerDir = (playerTransform.position - baseStartPos).normalized;

        float randomAngle = Random.Range(-30f, 30f);
        Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
        Vector3 finalDirection = randomRotation * toPlayerDir;

        for (int i = 0; i < birdCount; i++)
        {
            Vector3 offset = Vector3.zero;
            if (i == 1) offset = Vector3.left * 2f;
            if (i == 2) offset = Vector3.right * 2f;

            Vector3 spawnPos = baseStartPos + offset;

            GameObject bird = Instantiate(birdPrefab, spawnPos, Quaternion.LookRotation(finalDirection));
            activeBirds.Add(bird);
            StartCoroutine(DestroyBirdAfterTime(bird, birdLifetime));
        }
    }



    private void MoveBirds()
    {
        if (activeBirds.Count == 0) return;

        for (int i = activeBirds.Count - 1; i >= 0; i--)
        {
            GameObject bird = activeBirds[i];
            if (bird == null)
            {
                activeBirds.RemoveAt(i);
                continue;
            }

            bird.transform.position += bird.transform.forward * birdSpeed * Time.deltaTime;
        }
    }

    private IEnumerator DestroyBirdAfterTime(GameObject bird, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bird != null)
        {
            Destroy(bird);
        }
    }
}
