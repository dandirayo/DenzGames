using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreController : MonoBehaviour
{
    [Header("UI")]
    public Text score;
    public Text highScore;

    [Header("Score")]
    public ScoreController scoreController;

    // EFISIENSI: cache nilai terakhir, cuma set .text (yang men-trigger
    // rebuild layout Canvas) kalau nilainya benar-benar berubah.
    private int lastScoreShown = int.MinValue;
    private int lastHighScoreShown = int.MinValue;

    private void Update()
    {
        int currentScore = Mathf.FloorToInt(scoreController.GetCurrentScore());
        if (currentScore != lastScoreShown)
        {
            lastScoreShown = currentScore;
            score.text = currentScore.ToString();
        }

        int currentHighScore = ScoreData.highScore;
        if (currentHighScore != lastHighScoreShown)
        {
            lastHighScoreShown = currentHighScore;
            highScore.text = currentHighScore.ToString();
        }
    }
}
