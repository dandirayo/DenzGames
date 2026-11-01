using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameOverController : MonoBehaviour
{
    [Tooltip("Jeda sebelum klik dihitung sebagai restart, biar klik yang bikin mati tidak langsung reload.")]
    public float inputDelay = 0.3f;

    [Tooltip("Nama scene Main Menu (opsional, dipakai tombol Main Menu di panel Game Over).")]
    public string mainMenuSceneName = "MainMenu";

    private float shownAt;

    private void OnEnable()
    {
        shownAt = Time.unscaledTime;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.unscaledTime - shownAt < inputDelay) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
