using UnityEngine;

public class SpawnedObjectTracker : MonoBehaviour
{
    private SpawnObjects spawner;
    private bool isChest;

    public void Init(SpawnObjects spawner, bool isChest)
    {
        this.spawner = spawner;
        this.isChest = isChest;
    }

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnSpawnedObjectDestroyed(isChest);
    }
}
