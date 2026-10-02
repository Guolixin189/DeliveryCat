using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public GameObject settingPanel;

    public void OpenSettings()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }
}