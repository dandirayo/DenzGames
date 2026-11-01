using UnityEngine;

// EFISIENSI: high score sekarang persist antar sesi lewat PlayerPrefs,
// sebelumnya cuma "static int" yang hilang tiap kali aplikasi ditutup.
public static class ScoreData
{
    private const string HighScoreKey = "HighScore";

    private static int cachedHighScore = -1;

    public static int highScore
    {
        get
        {
            if (cachedHighScore < 0)
            {
                cachedHighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            }
            return cachedHighScore;
        }
        set
        {
            if (value == cachedHighScore) return;

            cachedHighScore = value;
            PlayerPrefs.SetInt(HighScoreKey, value);
            PlayerPrefs.Save();
        }
    }
}
