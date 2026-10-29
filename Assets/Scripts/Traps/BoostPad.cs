using UnityEngine;

// Boost pad: stepping on it gives a short speed boost.
public class BoostPad : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplyBoost();
    }
}
