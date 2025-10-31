using UnityEngine;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI highScoreTimeText;

    private void Start()
    {
        LoadHighScore();
    }

    private void LoadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        string highScoreTime = PlayerPrefs.GetString("HighScoreTime", "00:00:00");

        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore:D6}";

        if (highScoreTimeText != null)
            highScoreTimeText.text = $"Time: {highScoreTime}";
    }
}
