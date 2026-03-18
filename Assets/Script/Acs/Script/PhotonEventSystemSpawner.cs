// This script intentionally does not need to be attached to any GameObject in the scene.
// It will automatically run exactly once as soon as the game starts.
using UnityEngine;
using UnityEngine.EventSystems;

public class PhotonEventSystemSpawner : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SpawnEventSystem()
    {
        // 1. Hunt down any broken EventSystem that was left in the scene
        EventSystem existingEventSystem = Object.FindObjectOfType<EventSystem>();
        if (existingEventSystem != null)
        {
            Debug.LogWarning("Found an existing EventSystem (likely broken). Destroying it!");
            Object.DestroyImmediate(existingEventSystem.gameObject);
        }

        // 2. Spawn a new empty GameObject
        GameObject eventSystemGO = new GameObject("Runtime_EventSystem");

        // 3. Attach the core EventSystem component
        eventSystemGO.AddComponent<EventSystem>();
        
        // 4. Attach the legacy StandaloneInputModule. 
        // Because ProjectSettings allow BOTH Input Systems (activeInputHandler = 2), 
        // this legacy module works perfectly out-of-the-box for button clicks in builds.
        eventSystemGO.AddComponent<StandaloneInputModule>();
        
        // 5. Protect it so it doesn't get destroyed if you switch scenes later
        Object.DontDestroyOnLoad(eventSystemGO);

        Debug.Log("Successfully spawned a clean Runtime EventSystem autonomously!");
    }
}
