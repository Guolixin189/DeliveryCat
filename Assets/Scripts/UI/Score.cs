using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text scoreText;

    void Update()
    {
        scoreText.text = gameManager.score.ToString();
    }
}
