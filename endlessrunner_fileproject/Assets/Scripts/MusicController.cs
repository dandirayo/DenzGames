using UnityEngine;
using UnityEngine.UI;

// FITUR: BGM + tombol mute yang persist antar sesi via PlayerPrefs.
// Taruh di object dengan AudioSource (loop = true), bisa DontDestroyOnLoad
// kalau mau musik lanjut terus dari Main Menu ke Gameplay.
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    private const string MutedKey = "AudioMuted";

    public AudioClip backgroundMusic;
    [Tooltip("Opsional: tombol UI untuk toggle mute, teksnya otomatis di-update.")]
    public Text muteButtonLabel;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;

        bool muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        ApplyMuted(muted);

        audioSource.Play();
    }

    public void ToggleMute()
    {
        bool muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        ApplyMuted(!muted);
    }

    private void ApplyMuted(bool muted)
    {
        // Mute lewat AudioListener supaya berlaku global (musik + SFX lompat, dll),
        // bukan cuma AudioSource musik ini sendiri.
        AudioListener.volume = muted ? 0f : 1f;
        PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();

        if (muteButtonLabel != null)
        {
            muteButtonLabel.text = muted ? "Sound: Off" : "Sound: On";
        }
    }
}
