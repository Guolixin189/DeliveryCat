using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float remainingTime = 60f;
    public int score = 0;
    public int deliveredPackages = 0;
    public int starsEarned = 0;
    public GameObject gameOverPanel;
    public bool isGameOver = false;
    public static GameManager Instance;
    public TMP_Text finalScoreText;
    public TMP_Text gameOverText;
    public TMP_Text deliveredPackagesText;
    public GameObject completePanel;
    [Header("Score Sound Effects")]
    public AudioSource scoreAudioSource;
    public AudioClip positiveScoreSFX;
    public AudioClip negativeScoreSFX;

    [Header("Star Rating UI")]
    public Image[] stars;
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;
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
                GameOver("Time's up!", 50, 800);
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

    public void PackageDelivered()
    {
    if (isGameOver)
        return;

    deliveredPackages++;
    }

    public int CalculateStars()
        {
    if (deliveredPackages >= 5)
        return 3;
    else if (deliveredPackages >= 3)
        return 2;
    else if (deliveredPackages >= 1)
        return 1;
    else
        return 0;
    }

    // Best stars per level, shown on the level select screen.
    // Tutorial (level_1) uses 1/2/3; the official level uses 1/3/5.
    public static string BestStarsKey(string sceneName)
    {
        return "BestStars_" + sceneName;
    }

    public static int GetBestStars(string sceneName)
    {
        return PlayerPrefs.GetInt(BestStarsKey(sceneName), 0);
    }

    public static void SaveBestStars(string sceneName, int stars)
    {
        if (stars <= GetBestStars(sceneName))
            return;
        PlayerPrefs.SetInt(BestStarsKey(sceneName), stars);
        PlayerPrefs.Save();
    }

    public int CalculateTutorialStars()
    {
        if (deliveredPackages >= 3)
            return 3;
        else if (deliveredPackages >= 2)
            return 2;
        else if (deliveredPackages >= 1)
            return 1;
        else
            return 0;
    }
    public void GameOver(string gameOverMsg, float fontSize, float panelWidth)
    {
        if (isGameOver)
            return;

        isGameOver = true;
        starsEarned = CalculateStars();
        SaveBestStars(SceneManager.GetActiveScene().name, starsEarned);
        
        gameOverText.text = gameOverMsg;
        gameOverText.fontSize = fontSize;

        for (int i = 0; i < stars.Length; i++)
    {
        if (i < starsEarned)
        {
        stars[i].sprite = filledStarSprite;
        }
    else
        {
        stars[i].sprite = emptyStarSprite;
        }
    }

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
        deliveredPackagesText.text = "Packages Delivered: " + deliveredPackages;
        gameOverPanel.SetActive(true);
    }

    public void CompleteTutorial()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        SaveBestStars(SceneManager.GetActiveScene().name, CalculateTutorialStars());
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
