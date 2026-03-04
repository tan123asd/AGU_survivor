using UnityEngine;

/// <summary>
/// Lightweight marker component for player spawn points.
/// 
/// HOW TO USE:
/// 1. Create empty GameObjects at desired spawn locations in your scene.
/// 2. Attach this component to each one.
/// 3. Drag them into PlayerSpawner.spawnPoints[].
/// 
/// The green sphere Gizmo is only visible in the editor and has no runtime cost.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 1f, 0.4f, 0.5f);
    [SerializeField] private float gizmoRadius = 0.5f;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        // Outline
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);

        // Label
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (gizmoRadius + 0.2f),
            gameObject.name,
            new GUIStyle { normal = { textColor = Color.green }, fontSize = 10 }
        );
    }
#endif
}
