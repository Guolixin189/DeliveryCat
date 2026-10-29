using UnityEngine;

// Mud: 0.5x slow while inside, plus a 1s lingering "muddy" slow after leaving.
public class MudTrap : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplyMud();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplyMudLinger();
    }
}
