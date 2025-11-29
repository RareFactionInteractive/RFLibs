using UnityEngine;
using UnityEngine.SceneManagement;

namespace RFLibs.DependencyInjection.Unity
{
    /// <summary>
    /// Static manager for DI scene container lifecycle.
    /// Automatically initializes when Unity starts - no need to add to scenes manually.
    /// </summary>
    public static class DISceneManager
    {
        private static bool _initialized;

        /// <summary>
        /// Automatically called by Unity when the runtime starts.
        /// Subscribes to scene unload events to manage the DI scene container.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Debug.Log("DISceneManager initialized - scene container lifecycle management active");
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            // Clear scene container when scene is unloaded
            DI.ClearSceneContainer();
            Debug.Log($"Scene '{scene.name}' unloaded - DI scene container cleared");
        }
    }
}
