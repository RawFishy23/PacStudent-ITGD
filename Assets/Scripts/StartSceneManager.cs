using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
        int highScoreMain = PlayerPrefs.GetInt("HighScore_Main", 0);
        string highScoreTimeMain = PlayerPrefs.GetString("HighScoreTime_Main", "00:00:00");

        int highScoreLevel2 = PlayerPrefs.GetInt("HighScore_Level2", 0);
        string highScoreTimeLevel2 = PlayerPrefs.GetString("HighScoreTime_Level2", "00:00:00");

        if (highScoreText1 != null)
            highScoreText1.text = $"High Score: {highScoreMain:D6}";
        if (highScoreTimeText1 != null)
            highScoreTimeText1.text = $"Time: {highScoreTimeMain}";

        if (highScoreText2 != null)
            highScoreText2.text = $"High Score: {highScoreLevel2:D6}";
        if (highScoreTimeText2 != null)
            highScoreTimeText2.text = $"Time: {highScoreTimeLevel2}";
    }
}

