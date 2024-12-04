using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainScene : MonoBehaviour
{

    [SerializeField] private string mainSceneName = "Main";


    public void LoadMainScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(mainSceneName);
    }


    public void QuitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();
    }
}
