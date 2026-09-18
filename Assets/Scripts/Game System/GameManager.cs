using UnityEngine;
using UnityEngine.InputSystem;

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

        // Temporary test for score system
        if (Keyboard.current.pKey.wasPressedThisFrame){
            AddScore(100);
        }
    }

    public void AddScore(int points){
        score += points;
    }
}
