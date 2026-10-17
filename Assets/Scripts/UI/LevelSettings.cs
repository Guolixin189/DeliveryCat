using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public GameObject settingPanel;

    public void OpenSettings()
    {
        settingPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }
}