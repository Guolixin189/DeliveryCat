
using UnityEngine;

// Mud: 0.5x slow while inside, plus a 1s lingering slow after leaving.
public class MudTrap : MonoBehaviour
{
    [Header("Sound Effect")]
    public AudioClip cozySFX;
    public AudioSource audioSource;

    private void OnTriggerStay2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();

        if (cat != null)
        {
            cat.ApplyMud();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();

        if (cat != null)
        {
            cat.ApplyMudLinger();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();

        if (cat != null)
        {
            if (audioSource != null && cozySFX != null)
            {
                audioSource.PlayOneShot(cozySFX, 0.4f);
            }
        }
    }
}

