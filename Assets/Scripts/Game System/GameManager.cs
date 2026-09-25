using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float remainingTime = 60f;
    public int score = 0;
    public GameObject gameOverPanel;

    // Update is called once per frame
    void Update()
    {
        if(remainingTime>0){
            remainingTime -= Time.deltaTime;
        }
        else{
            remainingTime=0;
            gameOverPanel.SetActive(true);
        }

        
    }

    public void AddScore(int points){
        score += points;
    }

    public void RestartGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
