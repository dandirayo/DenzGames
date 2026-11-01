using UnityEngine;

// FITUR: pusat kontrol pause/resume. Ditaruh sebagai object tunggal di
// scene gameplay (boleh DontDestroyOnLoad kalau mau dipakai lintas scene).
public class GameManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pausePanel;

    // Dibaca script lain (mis. CharacterMoveController) via GameManager.IsPaused
    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        // reset static state tiap kali scene gameplay dimuat ulang
        IsPaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // jaga-jaga supaya timeScale tidak "nyangkut" 0 saat pindah scene
        Time.timeScale = 1f;
    }
}
