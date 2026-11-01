using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// FITUR: Main Menu. Taruh di scene baru "MainMenu" (lihat SETUP_GUIDE.md).
public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Nama scene gameplay, harus sama persis dengan nama file scene di Build Settings.")]
    public string gameplaySceneName = "EndlessRunnerTheGame";

    [Header("UI (opsional)")]
    public Text highScoreText;

    private void Start()
    {
        if (highScoreText != null)
        {
            highScoreText.text = ScoreData.highScore.ToString();
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f; // jaga-jaga kalau sebelumnya dari state pause
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
