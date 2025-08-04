using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

namespace dev.susybaka.raidsim
{
    public class LoadScene : MonoBehaviour
    {
        [SerializeField, Scene] private string scene;

        private void Start()
        {
            //Utilities.FunctionTimer.CleanUp();
            SceneManager.LoadScene(scene);
        }
    }
}