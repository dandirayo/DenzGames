using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    private int currentScore = 0;

    // Diisi otomatis kalau ada high score baru pada FinishScoring()
    public bool IsNewHighScore { get; private set; }

    private void Start()
    {
        //reset
        currentScore = 0;
        IsNewHighScore = false;
    }

    public float GetCurrentScore()
    {
        return currentScore;

    }

    public void IncreaseCurrentScore(int increment)
    {
        currentScore += increment;
    }

    // FITUR: dipakai CoinController untuk skor bonus dari koin
    public void AddBonusScore(int bonus)
    {
        currentScore += bonus;
    }

    public void FinishScoring()
    {
        if (currentScore > ScoreData.highScore)
        {
            ScoreData.highScore = currentScore; // otomatis tersimpan lewat PlayerPrefs
            IsNewHighScore = true;
        }
    }

}
