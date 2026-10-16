using UnityEngine;
using TMPro;

// 把 GameManager.score 实时显示到 UI 上（挂在 ScoreText 对象上即可）
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
