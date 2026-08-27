using UnityEngine;
using UnityEngine.SceneManagement;

// FITUR: tombol-tombol di panel Pause (Resume / Restart / Main Menu).
public class PauseMenuController : MonoBehaviour
{
    public GameManager gameManager;

    [Tooltip("Nama scene Main Menu, harus sama persis dengan nama file scene di Build Settings.")]
    public string mainMenuSceneName = "MainMenu";

    public void Resume()
    {
        if (gameManager != null) gameManager.Resume();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
