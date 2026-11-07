using UnityEngine;
using TMPro;


public class Timer : MonoBehaviour
{
    public GameManager gameManger;
    public TMP_Text timerText;

    void Start()
    {
        // Self-heal if the GameManager reference was dropped in the inspector.
        if (gameManger == null)
            gameManger = FindFirstObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManger == null || timerText == null)
            return;
        float time = gameManger.remainingTime;
        int totalSeconds = Mathf.CeilToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes +":" +seconds.ToString("00");
    }
}
