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

    void Awake(){
        Instance = this;
    }

    void Update()
    {
        if(!isGameOver)
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else
        {
            remainingTime = 0;
            isGameOver = true;
            finalScoreText.text = "Final Score: " + score;
            gameOverPanel.SetActive(true);
        }

        // Temporary test for score system
        if (!isGameOver && Keyboard.current.pKey.wasPressedThisFrame)
        {
            AddScore(100);
        }
    }
 }

    public void AddScore(int points)
    {
        if(isGameOver)
            return;

        score += points;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

    