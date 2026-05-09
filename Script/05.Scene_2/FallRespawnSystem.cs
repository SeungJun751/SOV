using UnityEngine;

public class FallRespawnSystem : MonoBehaviour
{
    [Header("리스폰 설정")]
    public float fallThreshold = -50f; 
    public Transform respawnPoint; 
    public float respawnDelay = 1f; 
    
    [Header("플레이어 참조")]
    public Transform player; 
    
    [Header("효과")]
    public GameObject respawnEffect; 
    public AudioClip respawnSound; 
    
    private Vector3 lastSafePosition; 
    private bool isRespawning = false;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (respawnPoint == null)
        {
            GameObject respawnObj = GameObject.Find("RespawnPoint");
            if (respawnObj != null)
                respawnPoint = respawnObj.transform;
        }
        
        if (player != null)
            lastSafePosition = player.position;
    }
    
    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("FallRespawnSystem: 플레이어가 null입니다.");
            return;
        }
        
        if (isRespawning) return;
        
        if (player.position.y < fallThreshold)
        {
            StartCoroutine(RespawnPlayer());
        }
        else if (player.position.y > fallThreshold + 5f) 
        {
            if (IsPlayerOnGround())
            {
                lastSafePosition = player.position;
            }
        }
    }
    
    private bool IsPlayerOnGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, 2f))
        {
            return hit.distance < 1.5f; 
        }
        return false;
    }
    
    private System.Collections.IEnumerator RespawnPlayer()
    {
        isRespawning = true;
        
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        yield return new WaitForSeconds(respawnDelay);

        Vector3 respawnPosition;
        if (respawnPoint != null)
        {
            respawnPosition = respawnPoint.position;
        }
        else
        {
            respawnPosition = lastSafePosition;
        }
        
        player.position = respawnPosition;
        
        if (rb != null)
        {
            rb.MovePosition(respawnPosition);
            rb.isKinematic = false; 
        }
        
        if (respawnEffect != null)
        {
            GameObject effect = Instantiate(respawnEffect, respawnPosition, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        if (respawnSound != null)
        {
            AudioSource.PlayClipAtPoint(respawnSound, respawnPosition);
        }
        
        Debug.Log($"✅ 플레이어가 리스폰되었습니다. 위치: {respawnPosition}");
        
        isRespawning = false;
    }
    
    public void SetSafePosition(Vector3 position)
    {
        lastSafePosition = position;
    }
    
    [ContextMenu("Manual Respawn")]
    public void ManualRespawn()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnPlayer());
        }
    }
}
