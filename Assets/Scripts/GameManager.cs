using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public List<Image> lifeImages;
    public TextMeshProUGUI ghostTimerText; 
    
    [Header("Game Settings")]
    public int lives = 3;
    private int score = 0;
    private float timer = 0f;
    private bool gameRunning = false;
    private float ghostTimer = 0f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (gameRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void StartGame()
    {
        score = 0;
        lives = lifeImages.Count; 
        timer = 0;
        gameRunning = true;
        UpdateScoreUI();
        UpdateLivesUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        UpdateLivesUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString("D6");
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeImages.Count; i++)
        {
            lifeImages[i].enabled = i < lives;
        }
    }

    private void GameOver()
    {
        gameRunning = false;
        Debug.Log("Game Over!");
    }

    public void CollectPellet(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    public void CollectPowerPellet(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    public void CollectBonus(int points)
    {
        score += points;
        UpdateScoreUI();
    }

        public void StartGhostTimer(float duration)
    {
        ghostTimer = duration;
        ghostTimerText.gameObject.SetActive(true);
    }

    public float GetRemainingGhostTime()
    {
        return ghostTimer;
    }
}
