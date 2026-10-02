using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(
            "MusicVolume",
            volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20
        );

        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(
            "SFXVolume",
            volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20
        );

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}