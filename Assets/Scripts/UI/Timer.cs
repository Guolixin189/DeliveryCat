using UnityEngine;
using TMPro;


public class Timer : MonoBehaviour
{
    public GameManager gameManger;
    public TMP_Text timerText;



    // Update is called once per frame
    void Update()
    {
        float time = gameManger.remainingTime;
        int totalSeconds = Mathf.CeilToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes +":" +seconds.ToString("00");
    }
}
