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
        StartCoroutine(StartGameWithSound("level_1"));
    }

    public void StartTutorial()
    {
        StartCoroutine(StartGameWithSound("level_1"));
    }

    private IEnumerator StartGameWithSound(string sceneName)
    {
        menuAudio.Play();

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(sceneName);
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