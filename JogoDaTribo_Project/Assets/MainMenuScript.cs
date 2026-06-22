using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private string hubScene = "HubUpgrade";

    public void StartGame()
    {
        SceneManager.LoadScene(hubScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}