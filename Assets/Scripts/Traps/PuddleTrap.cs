
using UnityEngine;

// Puddle: slippery controls while the player stays inside.
public class PuddleTrap : MonoBehaviour
{
    public float slipperyDuration = 0.3f;

    [Header("Sound Effect")]
    public AudioClip cozySFX;
    public AudioSource audioSource;

    private void OnTriggerStay2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();

        if (cat != null)
        {
            cat.ApplySlippery(slipperyDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();

        if (cat != null)
        {
            if (audioSource != null && cozySFX != null)
            {
                audioSource.PlayOneShot(cozySFX, 0.6f);
            }
        }
    }
}
