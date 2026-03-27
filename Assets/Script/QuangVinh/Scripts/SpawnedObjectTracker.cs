using UnityEngine;

public class SpawnedObjectTracker : MonoBehaviour
{
    private SpawnObjects spawner;

    public void Init(SpawnObjects spawner)
    {
        this.spawner = spawner;
    }

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnSpawnedObjectDestroyed();
    }
}
