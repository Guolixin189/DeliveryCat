using UnityEngine;

// Keeps the background music playing across scene loads instead of
// restarting it from the beginning every time a level starts or
// restarts. Attach to the BGM AudioSource object in every scene
// (MenuBGM in the Main Menu, MainBGM in level_1 and level_2).
// The first BGM to load becomes the persistent one. Any BGM that
// loads later destroys itself, so the music keeps playing from
// where it is instead of starting over.
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    static BackgroundMusic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // A BGM is already playing from an earlier scene.
            // Destroy this duplicate so the music is not restarted.
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource source = GetComponent<AudioSource>();
        if (source != null && !source.isPlaying)
            source.Play();
    }
}
