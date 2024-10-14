using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private int index = 1;

    void Start()
    {
        //Utilities.FunctionTimer.CleanUp();
        SceneManager.LoadScene(index);
    }
}
