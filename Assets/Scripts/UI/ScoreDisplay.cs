using UnityEngine;
using TMPro;

// Shows GameManager.score on the UI in real time (attach to the ScoreText object)
public class ScoreDisplay : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text scoreText;

    void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (gameManager != null && scoreText != null)
            scoreText.text = gameManager.score.ToString();
    }
}
