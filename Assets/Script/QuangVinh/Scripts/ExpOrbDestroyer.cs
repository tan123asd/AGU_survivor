using UnityEngine;

/// <summary>
/// Script này giúp destroy exp prefab khi b? trigger.
/// </summary>
public class ExpOrbDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[ExpOrbDestroyer] Exp '{gameObject.name}' ch?m vào '{collision.gameObject.name}'");
        
        if (collision.CompareTag("Player") || collision.gameObject.name.Contains("Player"))
        {
            Debug.Log($"[ExpOrbDestroyer] ? Ch?m vào Player! Destroy exp ngay...");
            Destroy(gameObject);
        }
    }
}
