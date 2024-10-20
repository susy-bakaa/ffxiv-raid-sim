using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;
    private static MusicLoader loader;
    private static string boundToSceneName;

    private void Awake()
    {
        if (loader == null)
        {
            loader = GetComponent<MusicLoader>();
        }

        // Check if there is already an instance of this object
        if (instance == null)
        {
            // This is the first instance, so make it persist across scenes
            instance = this;
            boundToSceneName = SceneManager.GetActiveScene().name;
            if (loader != null)
                loader.Load();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a different scene is loaded, destroy this instance
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (boundToSceneName != currentSceneName)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update the scene name when a new scene is loaded
        if (scene.name != boundToSceneName)
        {
            Destroy(gameObject);  // Destroy this object when switching to a new scene
        }
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
