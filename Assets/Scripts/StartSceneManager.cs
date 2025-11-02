using UnityEngine;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    [Header("Level 1")]
    public TextMeshProUGUI highScoreText1;
    public TextMeshProUGUI highScoreTimeText1;

    [Header("Level 2")]
    public TextMeshProUGUI highScoreText2;
    public TextMeshProUGUI highScoreTimeText2;

    private void Start()
    {
        LoadHighScore();
    }

    private void LoadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        string highScoreTime = PlayerPrefs.GetString("HighScoreTime", "00:00:00");

        if (highScoreText1 != null)
            highScoreText1.text = $"High Score: {highScore:D6}";

        if (highScoreTimeText1 != null)
            highScoreTimeText1.text = $"Time: {highScoreTime}";
    }
}
