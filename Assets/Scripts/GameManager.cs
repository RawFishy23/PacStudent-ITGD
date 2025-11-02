using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public List<Image> lifeImages;
    public TextMeshProUGUI ghostTimerText;


    [Header("Round Start Countdown")]
    public int countdownTime;
    public TextMeshProUGUI roundCountdownText;
    public Image startBlockerImage;

    [Header("Game Over UI")]
    public TextMeshProUGUI gameOverText;
    public Image blockerImage;
    public string mainMenuSceneName = "StartScene";


    [Header("Game Settings")]
    private int score = 0;
    private float timer = 0f;
    private bool gameRunning = false;
    private float ghostTimer = 0f;

    public bool allowInput = false;

    public Tilemap pelletTilemap; // assign your pellet tilemap
    public TileBase normalPellet;
    public TileBase powerPellet;

    public int ScaredGhostsCount { get; private set; } = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();  
    }

    private void Update()
    {
        if (gameRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }

        if (ghostTimer > 0)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0)
            {
                ghostTimer = 0;
                if (ghostTimerText != null)
                    ghostTimerText.gameObject.SetActive(false);
            }
            else
            {
                if (ghostTimerText != null)
                    ghostTimerText.text = Mathf.Ceil(ghostTimer).ToString(); 
            }
        }
    }

    #region Game Flow

    public void StartGame()
    {
        score = 0;
        timer = 0f;
        gameRunning = false;
        allowInput = false;

        UpdateScoreUI();

        foreach (var heart in lifeImages)
            heart.enabled = true;

        StartCoroutine(RoundCountdownCoroutine());
    }

    private IEnumerator RoundCountdownCoroutine()
    {
        while (countdownTime > 0)
        {
            roundCountdownText.text = countdownTime.ToString();
            if (blockerImage != null) blockerImage.gameObject.SetActive(true);
            if (roundCountdownText) roundCountdownText.gameObject.SetActive(true);

            yield return new WaitForSeconds(1f);

            countdownTime--;
        }

        roundCountdownText.text = "GO!";

        yield return new WaitForSeconds(1f);

        if (roundCountdownText != null) roundCountdownText.gameObject.SetActive(false);
        if (blockerImage) blockerImage.gameObject.SetActive(false);

        gameRunning = true;
        allowInput = true;

        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        foreach (var ghost in ghosts)
        {
            ghost.canMove = true;
        }

        Debug.Log("Countdown finished! Game started.");
    }

    public bool AreAllPelletsEaten()
    {
        foreach (Vector3Int pos in pelletTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = pelletTilemap.GetTile(pos);
            if (tile == normalPellet || tile == powerPellet)
                return false;
        }
        return true;
    }

    #endregion

    #region Score & Lives

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void LoseLife()
    {
        for (int i = lifeImages.Count - 1; i >= 0; i--)
        {
            if (lifeImages[i].enabled)
            {
                lifeImages[i].enabled = false;
                if (i == 0) GameOver();
                break;
            }
        }
    }

    #endregion

    #region UI Updates

    private void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = score.ToString("D6");
    }

    private void UpdateTimerUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            int milliseconds = Mathf.FloorToInt((timer * 100) % 100);
            timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        }
    }

    #endregion

    public void GameOver()
    {
        gameRunning = false;
        allowInput = false;

        SaveHighScore();

        if (gameOverText != null) gameOverText.gameObject.SetActive(true);
        if (blockerImage != null) blockerImage.gameObject.SetActive(true);

        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SaveHighScore()
    {
        int previousHigh = PlayerPrefs.GetInt("HighScore", 0);

        if (score > previousHigh)
        {
            PlayerPrefs.SetInt("HighScore", score);

            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            int milliseconds = Mathf.FloorToInt((timer * 100) % 100);

            string formattedTime = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
            PlayerPrefs.SetString("HighScoreTime", formattedTime);

            PlayerPrefs.Save();
            Debug.Log($"New High Score Saved: {score} ({formattedTime})");
        }
    }

    #region Ghost Timer

    public void StartGhostTimer(float duration)
    {
        ghostTimer = duration;
        if (ghostTimerText != null)
            ghostTimerText.gameObject.SetActive(true);
    }

    public float GetRemainingGhostTime() => ghostTimer;

        public void GhostBecameScared()
    {
        ScaredGhostsCount++;
        if (AudioPlayer.Instance != null)
            AudioPlayer.Instance.SwitchState(BGMState.Scared);
    }

    public void GhostReturnedToNormal()
    {
        ScaredGhostsCount = Mathf.Max(0, ScaredGhostsCount - 1);
        if (ScaredGhostsCount == 0)
        {
            if (AudioPlayer.Instance != null)
                AudioPlayer.Instance.SwitchState(BGMState.Normal);
        }
    }

    #endregion
}
