using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject howToPlayPanel;
    public GameObject settingPanel;
    public AudioSource menuAudio;

    void Start()
    {
        // Play a short hover sound whenever the mouse moves over any
        // button on the main menu (level buttons, How To Play, Settings,
        // company buttons, Back buttons). Uses the MENU_HOVER_2 clip
        // already assigned to menuAudio.
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => PlayHoverSound());
            trigger.triggers.Add(entry);
        }
    }

    void PlayHoverSound()
    {
        if (menuAudio != null && menuAudio.clip != null)
            menuAudio.PlayOneShot(menuAudio.clip);
    }

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