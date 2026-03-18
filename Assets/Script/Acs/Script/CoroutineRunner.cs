using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Utility that runs a delayed callback on any ACTIVE GameObject,
/// independent of the calling MonoBehaviour's own active state.
/// Uses WaitForSecondsRealtime so it works even when Time.timeScale = 0
/// (e.g. while the Upgrade Panel is open).
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    /// <summary>
    /// Adds a temporary CoroutineRunner to <paramref name="host"/> and runs
    /// <paramref name="callback"/> after <paramref name="delay"/> real-time seconds.
    /// Safe to call from inactive MonoBehaviours.
    /// </summary>
    public static void RunDelayed(GameObject host, float delay, Action callback)
    {
        if (host == null || !host.activeInHierarchy)
        {
            Debug.LogWarning("[CoroutineRunner] Host is null or inactive — callback skipped.");
            return;
        }

        CoroutineRunner runner = host.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(runner.DelayedAction(delay, callback));
    }

    private IEnumerator DelayedAction(float delay, Action callback)
    {
        yield return new WaitForSecondsRealtime(delay);
        callback?.Invoke();
        if (this != null) Destroy(this); // clean up once done
    }
}
