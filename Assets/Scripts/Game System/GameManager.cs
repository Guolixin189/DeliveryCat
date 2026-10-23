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
    public GameObject completePanel;
    [Header("Score Sound Effects")]
    public AudioSource scoreAudioSource;
    public AudioClip positiveScoreSFX;
    public AudioClip negativeScoreSFX;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
    if (SceneManager.GetActiveScene().name == "level_2")
    {
        remainingTime = GameSettings.SelectedDuration;
    }
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
                if (SceneManager.GetActiveScene().name == "level_1")
            {
                 CompleteTutorial();
             }
            else
            {
                GameOver("Time's up!", 50, 400);
            }
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

    if (scoreAudioSource == null)
        return;

    if (points > 0 && positiveScoreSFX != null)
    {
        scoreAudioSource.PlayOneShot(positiveScoreSFX);
    }
    else if (points < 0 && negativeScoreSFX != null)
    {
        scoreAudioSource.PlayOneShot(negativeScoreSFX);
    }
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

    public void CompleteTutorial()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        completePanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
    
    public void PlayGameAfterTutorial()
    {
    Time.timeScale = 1f;

    GameSettings.OpenCompanySelectOnMenu = true;

    SceneManager.LoadScene("Main Menu");
    }
}
