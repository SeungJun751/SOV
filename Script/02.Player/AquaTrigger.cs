using UnityEngine;

public class AquaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PlayerSwitcher switcher = FindFirstObjectByType<PlayerSwitcher>();
            if (switcher != null)
            {
                switcher.EnableSwitch(); 
            }

            Destroy(gameObject); 
        }
    }
}
