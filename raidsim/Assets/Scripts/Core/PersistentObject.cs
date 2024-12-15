using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance; // The single instance
    private static string boundToSceneName;   // The scene the instance is bound to

    private void Awake()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (instance == null)
        {
            // This is the first instance; bind it to the current scene
            instance = this;
            boundToSceneName = currentSceneName;

            HandleMusicLoader();

            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // Check if the existing instance is already bound to the current scene
            if (boundToSceneName == currentSceneName)
            {
                // Another instance exists, but it's correctly bound; destroy this duplicate
                Destroy(gameObject);
            }
            else
            {
                // The existing instance is bound to a different scene; replace it
                Destroy(instance.gameObject);

                instance = this;
                boundToSceneName = currentSceneName;

                HandleMusicLoader();

                DontDestroyOnLoad(gameObject);
            }
        }
    }

    private void HandleMusicLoader()
    {
        MusicLoader loader = GetComponent<MusicLoader>();
        loader?.Load();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance == this && boundToSceneName != scene.name)
        {
            // If this object is bound to a different scene, destroy it
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
