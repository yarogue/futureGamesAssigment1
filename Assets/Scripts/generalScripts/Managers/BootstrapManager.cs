using UnityEngine;
using UnityEngine.SceneManagement;

namespace generalScripts.Managers
{
    public enum SceneIndex
    {
        MainMenu = 1,
        GameScene = 2
    }

    public class BootstrapManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SceneIndex firstScene = SceneIndex.MainMenu;

        [SerializeField] private bool autoLoadFirstScene = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {

            var servicesValid = ValidateServices();

            if (!servicesValid)
            {
                return;
            }

            if (autoLoadFirstScene)
            {
                LoadFirstScene();
            }
        }

        private bool ValidateServices()
        {
            var allValid = true;

            return allValid;
        }

        private void LoadFirstScene()
        {
            var sceneIndex = (int)firstScene;
            SceneManager.LoadScene(sceneIndex);
        }
        
        private void ReloadBootstrap()
        {
            SceneManager.LoadScene(0);
        }
    }
}
