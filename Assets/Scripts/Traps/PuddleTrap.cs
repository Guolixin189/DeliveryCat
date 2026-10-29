using UnityEngine;

// Puddle: while the player stays inside, steering wobbles and the cat
// cannot walk straight (slippery controls, not a hard stun).
public class PuddleTrap : MonoBehaviour
{
    public float slipperyDuration = 0.3f; // refreshed every physics step while inside

    private void OnTriggerStay2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplySlippery(slipperyDuration);
    }
}
