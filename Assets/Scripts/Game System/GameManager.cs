using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float remainingTime = 60f;
    public int score = 0;
    public GameObject gameOverPanel;
    public bool isGameOver = false;
    public static GameManager Instance;
    public TMP_Text finalScoreText;
    public TMP_Text gameOverText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else
            {
                remainingTime = 0;
                GameOver("Time's up!", 50, 400);
            }

            // Temporary test for score system
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                AddScore(100);
            }
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver)
            return;

        score += points;
    }

    public void GameOver(string gameOverMsg, float fontSize, float panelWidth)
    {
        if (isGameOver)
            return;

        isGameOver = true;

        gameOverText.text = gameOverMsg;
        gameOverText.fontSize = fontSize;

        RectTransform panel = gameOverPanel.GetComponent<RectTransform>();
        panel.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            panelWidth
        );

        RectTransform textWidth = gameOverText.GetComponent<RectTransform>();
        textWidth.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            panelWidth - 30
        );

        finalScoreText.text = "Final Score: " + score;
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}