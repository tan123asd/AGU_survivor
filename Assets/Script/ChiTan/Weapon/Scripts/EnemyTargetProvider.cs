using System.Collections.Generic;
using UnityEngine;

public interface IEnemyTargetProvider
{
    GameObject FindNearestEnemy(Vector2 origin, float range);
    int FindEnemiesInRange(Vector2 origin, float range, List<GameObject> results);
}

public sealed class PhysicsEnemyTargetProvider : IEnemyTargetProvider
{
    private readonly List<Collider2D> _hits;
    private readonly ContactFilter2D _contactFilter;
    private readonly string _enemyTag;

    public PhysicsEnemyTargetProvider(int bufferSize = 64, string enemyTag = "Enemy")
    {
        _hits = new List<Collider2D>(Mathf.Max(8, bufferSize));
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = false;
        filter.useDepth = false;
        filter.useNormalAngle = false;
        filter.useTriggers = true;
        _contactFilter = filter;
        _enemyTag = enemyTag;
    }

    public GameObject FindNearestEnemy(Vector2 origin, float range)
    {
        _hits.Clear();
        int count = Physics2D.OverlapCircle(origin, range, _contactFilter, _hits);
        GameObject nearest = null;
        float nearestSqrDistance = range * range;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _hits[i];
            if (col == null) continue;

            GameObject candidate = col.gameObject;
            if (!candidate.activeInHierarchy || !candidate.CompareTag(_enemyTag)) continue;

            float sqrDistance = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    public int FindEnemiesInRange(Vector2 origin, float range, List<GameObject> results)
    {
        if (results == null) return 0;

        results.Clear();

        _hits.Clear();
        int count = Physics2D.OverlapCircle(origin, range, _contactFilter, _hits);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = _hits[i];
            if (col == null) continue;

            GameObject candidate = col.gameObject;
            if (!candidate.activeInHierarchy || !candidate.CompareTag(_enemyTag)) continue;

            if (!results.Contains(candidate))
            {
                results.Add(candidate);
            }
        }

        return results.Count;
    }
}
