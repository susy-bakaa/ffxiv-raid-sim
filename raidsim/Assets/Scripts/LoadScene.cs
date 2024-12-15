using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField, Scene] private string scene;

    void Start()
    {
        //Utilities.FunctionTimer.CleanUp();
        SceneManager.LoadScene(scene);
    }
}
