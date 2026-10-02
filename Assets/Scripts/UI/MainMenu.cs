using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject howToPlayPanel;
    public GameObject settingPanel;
    public AudioSource menuAudio;

    public void StartGame()
    {
        StartCoroutine(StartGameWithSound());
    }

    private IEnumerator StartGameWithSound()
    {
        menuAudio.Play();

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene("level_1");
    }

    public void OpenHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }
}